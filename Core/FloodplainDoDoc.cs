using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TIME.ManagedExtensions;
using TIME.Science;
using TIME.Science.Mathematics.Functions;

namespace FlowMatters.Source.DODOC.Core
{
    public class FloodplainDoDoc : DoDocModel
    {
        /// <summary>
        /// Determine whether the Floodplain has been initialised or not
        /// </summary>
        private bool _initialised;

        /// <summary>
        /// The state of the area modelled in the Floodplain
        /// 
        /// The Zones collection has a very specific state. 
        /// This state has been carried over (presumably) from the FORTRAN implemenation.
        /// The rules are...
        /// 1. Dry zones are grouped together and come first in the collection
        /// 2. Dry zones are ordered by DECREASING area
        /// 3. Wet zones are grouped together and come last in the collection
        /// 4. Wet zones are ordered by INCREASING area
        /// </summary>
        public List<FloodplainData> Zones { get; private set;  }


        /// <summary>
        /// Tracks the prior surface area of the storage.
        /// Used to determine whether there has been an increase of decrease in the amount of wet area coverage
        /// </summary>
        protected double PreviousArea { get; set; }

        public override int ZoneCount { get { return Zones.Count; } }

        public override int CountInundatedZones
        {
            get
            {
                return Zones.Count(z => z.Wet);
            }
        }

        public override int CountDryZones
        {
            get
            {
                return Zones.Count(z => z.Dry);
            }
        }

        public override double LeafDryMatterReadilyDegradable
        {
            get
            {
                return Zones.Sum(z=>z.DryMassKg(z.LeafDryMatterReadilyDegradable));
            }
        }

        public override double LeafDryMatterNonReadilyDegradable
        {
            get
            {
                return Zones.Sum(z => z.DryMassKg(z.LeafDryMatterNonReadilyDegradable));
            }
        }

        public override double LeafWetMatterReadilyDegradable
        {
            get
            {
                return Zones.Sum(z => z.WetMassKg(z.LeafDryMatterReadilyDegradable));
            }
        }

        public override double LeafWetMatterNonReadilyDegradable
        {
            get
            {
                return Zones.Sum(z => z.WetMassKg(z.LeafDryMatterNonReadilyDegradable));
              
            }
        }

        public override double LeafDryMatterReadilyDegradableRate
        {
            get
            {
                return Zones.Sum(z => z.DryMassKg(z.LeafDryMatterReadilyDegradable)) / Zones.Sum(z => z.DryMassKg(1.0));
            }
        }

        public override double LeafDryMatterNonReadilyDegradableRate
        {
            get
            {
                return Zones.Sum(z => z.DryMassKg(z.LeafDryMatterNonReadilyDegradable)) / Zones.Sum(z => z.DryMassKg(1.0));
            }
        }

        public override double TotalDryMattergm2
        {
            get
            {
                return (Zones.Sum(z => z.DryMassKg(z.LeafDryMatterReadilyDegradable)) + Zones.Sum(z => z.DryMassKg(z.LeafDryMatterNonReadilyDegradable))) / Zones.Sum(z => z.DryMassKg(1.0))/10.0; // 10 is to convert from kg/ha to g/m2
            }
        }
        public override double FloodplainDryAreaHa
        {
            get { return Zones.Sum(z => z.DryMassKg(1.0)); }
        }

        public override double FloodplainWetAreaHa
        {
            get { return Zones.Sum(z => z.WetMassKg(1.0)); }
        }

        public override double AverageZoneAccumulation => Zones.Count == 0 ? 0 : Zones.Sum(z => z.LeafAccumulation) / ZoneCount;

        public override double TotalZoneAccumulation => Zones.Sum(z => z.LeafAccumulation);

        public int FloodCounter { get; set; }

        public FloodplainDoDoc()
        {
            _initialised = false;
        }


        /// <summary>
        /// The objective of this procedure is to reorder the Zone list so that all Dry zones come before Wet zones
        /// 
        /// Note: this method seems to be 
        /// </summary>
        private void GroupDryZones()
        {
            if (Zones.Count <= 1)
                return;

            for (int i = 0; i < (Zones.Count-1); i++)
            {
                // Find the first spot where a where a Wet zone comes after a Dry zone
                if (Zones[i].Dry && Zones[i + 1].Wet)
                {
                    for (int j = i; j < (Zones.Count - 1); j++)
                    {
                        // Find the first Dry zone to the follow the wet zone and put it back at index 'i+1'
                        // This should be in front of all Wet zones
                        if (Zones[j].Wet && Zones[j + 1].Dry)
                        {
                            FloodplainData tmp = Zones[j + 1];
                            Zones.RemoveAt(j + 1);
                            Zones.Insert(i+1,tmp);
                            
                            // Only move one Dry zone then break back to the outer loop
                            break;
                        }
                    }
                }
            }
        }

        private void RemoveInsignificantZones()
        {
            // label 200
            if (Zones.Count <= 1)
                return;

            for (int i = 0; i < Zones.Count; i++)
            {
                if (Zones[i].CumulativeAreaM2 < 1)
                {
                    Zones.RemoveAt(i);
                    i = 0;
                }
            }
        }


        /// <summary>
        /// Main processing update of the Zone state
        /// </summary>
        private void UpdateFloodedAreas()
        {
            // Find whether the water level has increased or decreased
            double deltaArea = Areal.Area - PreviousArea;
            PreviousArea = Areal.Area;

            if (WorkingVolume.Less(0.0)) // Storage vs Flood storage???
            {
                deltaArea = 0.0; // +++TODO Hack hack hack...
            }

            // Decide what to do based on how the area has changed
            if (deltaArea > 0)
            {
                IncreaseFloodedZones(deltaArea);
            }
            else if (deltaArea < 0)
            {
                ContractFloodedZones(Math.Abs(deltaArea));
            }
            else if (Areal.Area.Greater(0.0))
            {
                StableFloodZones();
            }
            else
            {
                EndOfFlood();
            }


            RemoveInsignificantZones();

            // Sort the Zones so the collection is back in the required order
            GroupDryZones();

            if(Debug) PrintZones(deltaArea);
        }

        private void EndOfFlood()
        {
            if (FloodCounter == 0)
                return;

            for(int i = 0;i< Zones.Count; i++)
            {
                var zone = Zones[i];
                if (Areal.Area < zone.CumulativeAreaM2 && zone.Wet)
                {
                    if (i == (Zones.Count - 1))
                    {
                        var newDryZone = new FloodplainData(false);
                        newDryZone.CumulativeAreaM2 = zone.CumulativeAreaM2;
                        newDryZone.ElevationM = zone.ElevationM;
                        newDryZone.LeafDryMatterReadilyDegradable = zone.LeafDryMatterReadilyDegradable;
                        newDryZone.LeafDryMatterNonReadilyDegradable = zone.LeafDryMatterNonReadilyDegradable;
                        newDryZone.ZoneAreaM2 = zone.CumulativeAreaM2;
                        Zones.Add(newDryZone);
                        break;
                    }
                    else
                    {
                        int removeWetZones = 1;
                        int lastFloodZone = i;
                        for (int j = (i + 1); j < Zones.Count; j++)
                        {
                            if (Zones[j].Wet)
                            {
                                removeWetZones++;
                                lastFloodZone = j;
                            }
                        }

                        var shortleaf1 = 0d;
                        var shortleaf2 = 0d;
                        for (int j = (Zones.Count - 1); j >= (i + 1); j--)
                        {
                            var floodZone = Zones[j];
                            if (floodZone.Wet)
                            {
                                shortleaf1 += floodZone.LeafDryMatterReadilyDegradable*(floodZone.CumulativeAreaM2 - Areal.Area)/
                                              Zones[lastFloodZone].CumulativeAreaM2;
                                shortleaf2 += floodZone.LeafDryMatterNonReadilyDegradable * (floodZone.CumulativeAreaM2 - Areal.Area) /
                                              Zones[lastFloodZone].CumulativeAreaM2;
                            }
                        }
                        shortleaf1 += zone.LeafDryMatterReadilyDegradable*(zone.CumulativeAreaM2 - Areal.Area)/
                                      Zones[lastFloodZone].CumulativeAreaM2;
                        shortleaf2 += zone.LeafDryMatterNonReadilyDegradable* (zone.CumulativeAreaM2 - Areal.Area) /
                                      Zones[lastFloodZone].CumulativeAreaM2;

                        //TODO - This code looks strange. Is the newDryZone ever used? Is this whole block meaningless?
                        var newDryZone = new FloodplainData(false);
                        newDryZone.CumulativeAreaM2 = Zones[lastFloodZone].CumulativeAreaM2;
                        newDryZone.ElevationM = Zones[lastFloodZone].ElevationM;
                        newDryZone.LeafDryMatterReadilyDegradable = shortleaf1;
                        newDryZone.LeafDryMatterNonReadilyDegradable = shortleaf2;
                        newDryZone.ZoneAreaM2 = newDryZone.CumulativeAreaM2;
                        break;
                    }
                }
            }

            /* Marker 150 */
            if(Zones.Count > 1)
            {
                for (int i = 0; i < Zones.Count; i++)
                {
                    if (Zones[i].Wet || Zones[i].CumulativeAreaM2 < 1)
                    {
                        Zones.RemoveAt(i);
                        i--;
                    }
                }
            }

            for (int i = 0; i < (Zones.Count - 1); i++)
            {
                if (!(Zones[i].CumulativeAreaM2 - Zones[i + 1].CumulativeAreaM2).EqualWithTolerance(Zones[i].ZoneAreaM2))
                { // Why the if?
                    Zones[i].ZoneAreaM2 = Zones[i].CumulativeAreaM2 - Zones[i + 1].CumulativeAreaM2;
                }
            }

            if (!Zones.Last().CumulativeAreaM2.EqualWithTolerance(Zones.Last().ZoneAreaM2))
            { // Why the if?
                Zones.Last().ZoneAreaM2 = Zones.Last().CumulativeAreaM2;
            }

            FloodCounter = 0;
        }


        /// <summary>
        /// If the area has not changed, the Zones do not change state
        /// </summary>
        private void StableFloodZones()
        {
            FloodCounter++;
        }


        /// <summary>
        /// The area has reduced.
        /// One of the Wet zones will now be reduced to cover the new resulting area.
        /// Possibly some of the Wet zones will be removed entirely.
        /// A new Dry Zone will be created to cover the area that is now dry.
        /// </summary>
        private void ContractFloodedZones(double reducedArea)
        {
            FloodCounter++;

            for (int i = 0; i < Zones.Count; i++)
            {
                var zone = Zones[i];

                // Skip past all the Dry zones
                // We are looking for the Wet zone covers the current elevation
                // This is the zone that will be split into separate Wet and Dry zones

                // Since Wet zones are ordered by INCREASING area, 
                // we search through the collection looking for the FIRST to have an area GREATER than the current floodplain area
                if (Areal.Area.GreaterOrEqual(zone.CumulativeAreaM2) || zone.Dry)
                    continue;

                int removeWetZones = 0;
                int lastFloodZone = i;

                if (Areal.Area.EqualWithTolerance(0.0))
                {
                    lastFloodZone = i;
                    removeWetZones = 1;
                }
                else
                {
                    // We want to find the range of Wet zones that need to be removed as they will be replaced by a Dry zone 
                    for (int j = (i + 1); j < Zones.Count; j++)
                    {
                        if (Zones[j].Wet)
                        {
                            removeWetZones++;
                            lastFloodZone = j;
                        }
                    }
                }

                var shortleaf1 = 0d;
                var shortleaf2 = 0d;
                for (int j = (Zones.Count - 1); j >= (i + 1); j--)
                {
                    var zoneJ = Zones[j];
                    if (zoneJ.Wet)
                    {
                        double ratio = zoneJ.ZoneAreaM2/reducedArea;
                        shortleaf1 += (zoneJ.LeafDryMatterReadilyDegradable*ratio);
                        shortleaf2 += (zoneJ.LeafDryMatterNonReadilyDegradable*ratio);
                    }
                }
                double ratioReducedArea = (zone.CumulativeAreaM2 - Areal.Area)/reducedArea;
                shortleaf1 += (zone.LeafDryMatterReadilyDegradable*ratioReducedArea);
                    // !use remaining area
                shortleaf2 += (zone.LeafDryMatterNonReadilyDegradable*ratioReducedArea);

                // Create a new Dry zone to cover the area of all Wet zones that have been removed or resized.
                var newZone = new FloodplainData(false);
                newZone.CumulativeAreaM2 = Zones[lastFloodZone].CumulativeAreaM2;
                newZone.ElevationM = Zones[lastFloodZone].ElevationM;

                newZone.LeafDryMatterReadilyDegradable = shortleaf1;
                newZone.LeafDryMatterNonReadilyDegradable = shortleaf2;
                newZone.ZoneAreaM2 = reducedArea;
                Zones.Add(newZone);

                // Resize the Wet zone that will be split
                zone.ZoneAreaM2 -= zone.CumulativeAreaM2 - Areal.Area;
                zone.CumulativeAreaM2 = Areal.Area;
                zone.ElevationM = Areal.Elevation;

                if (zone.ZoneAreaM2.Less(0.0))
                    zone.ZoneAreaM2 = Areal.Area;

                // Remove the Wet zones that have been replaced
                if (Areal.Area.EqualWithTolerance(0.0))
                    Zones.RemoveRange(i, removeWetZones);
                else
                    Zones.RemoveRange(i + 1, removeWetZones);
                return;
            }


            // TODO - I don't know how this code is reached
            // TODO - It seems to me that to pass the IF statement below, it would also not have gotten out of the prior loop...
            if (Areal.Area.Less(Zones.Last().CumulativeAreaM2) && Zones.Last().Wet)
            {
                var last = Zones.Last();

                var newZone = new FloodplainData(false);
                newZone.CumulativeAreaM2 = last.CumulativeAreaM2;
                newZone.ElevationM = last.ElevationM;

                newZone.LeafDryMatterReadilyDegradable = last.LeafDryMatterReadilyDegradable;
                newZone.LeafDryMatterNonReadilyDegradable = last.LeafDryMatterNonReadilyDegradable;
                newZone.ZoneAreaM2 = reducedArea;
                Zones.Add(newZone);

                last.ZoneAreaM2 -= last.CumulativeAreaM2 - Areal.Area;
                last.CumulativeAreaM2 = Areal.Area;
                last.ElevationM = Areal.Elevation;

                if (last.ZoneAreaM2.Less(0.0))
                {
                    last.ZoneAreaM2 = Areal.Area;
                } // TODO ++ Common code...
            }
        }

        
        /// <summary>
        /// The area has increased.
        /// A Dry zone will be reduced in size as the water level encroaches on it
        /// Possible several Dry zones will be removed entirely as they are covered.
        /// A new Wet zone will cover the area that is now wet
        /// </summary>
        private void IncreaseFloodedZones(double newarea)
        {
            FloodCounter++;
            int minDryZone=-1;
            double shortleaf1, shortleaf2;
            if (Zones.Count == 1)
            {
                shortleaf1 = Zones[0].LeafDryMatterReadilyDegradable;
                shortleaf2 = Zones[0].LeafDryMatterNonReadilyDegradable;
                minDryZone = 0; // Departure from Fortran. To override ZoneAreaM2
            }
            else
            {
                minDryZone = Zones.Count-1;

                // We are looking for the Dry zone covers the current elevation
                // This is the zone that will be split into separate Wet and Dry zones

                // Since Dry zones are ordered by DECREASING area, 
                // we search through the collection in REVERSE,
                // looking for the zone with area GREATER than the current floodplain area
                // Any Dry zone with an area Less than the current floodplain area will be removed
                for (int i = (Zones.Count - 1); i >= 0; i--)
                {
                    if (Areal.Area.Less(Zones[i].CumulativeAreaM2) && Zones[i].Dry)
                    {
                        minDryZone = i;
                        break;
                    }
                }

                if (minDryZone == (Zones.Count - 1))
                {
                    shortleaf1 = Zones.Last().LeafDryMatterReadilyDegradable;
                    shortleaf2 = Zones.Last().LeafDryMatterNonReadilyDegradable;
                }
                else if (Zones[minDryZone + 1].Wet)
                {
                    shortleaf1 = Zones[minDryZone].LeafDryMatterReadilyDegradable;
                    shortleaf2 = Zones[minDryZone].LeafDryMatterNonReadilyDegradable;
                }
                else
                {
                    shortleaf1 = 0d;
                    shortleaf2 = 0d;
                    
                    for (int i = (Zones.Count - 1); i >= minDryZone; i--)
                    {
                        if (Zones[i].Wet) continue;

                        double ratioNewArea = (Zones[i].CumulativeAreaM2 - (Areal.Area - newarea))/newarea;
                        shortleaf1 = shortleaf1 +
                                     Zones[i].LeafDryMatterReadilyDegradable*ratioNewArea; // !use ratio;
                        shortleaf2 = shortleaf2 +
                                     Zones[i].LeafDryMatterNonReadilyDegradable*ratioNewArea;
                        for (int j = (i - 1); j >= (minDryZone + 1); j--)
                        {
                            if (Zones[j].Dry)
                            {
                                double ratio2 = Zones[j].ZoneAreaM2/newarea;
                                shortleaf1 = shortleaf1 +
                                             (Zones[j].LeafDryMatterReadilyDegradable*ratio2);
                                shortleaf2 = shortleaf2 +
                                             (Zones[j].LeafDryMatterNonReadilyDegradable*ratio2);
                            }
                        }
                        double anotherRatio = (Areal.Area - Zones[minDryZone + 1].CumulativeAreaM2)/newarea;
                        shortleaf1 = shortleaf1 + Zones[minDryZone].LeafDryMatterReadilyDegradable*anotherRatio; //  !use ratio
                        shortleaf2 = shortleaf2 + Zones[minDryZone].LeafDryMatterNonReadilyDegradable*anotherRatio;
                        break;
                    }
                }
            }

            var newZone = new FloodplainData(true);
            newZone.CumulativeAreaM2 = Areal.Area;
            newZone.ElevationM = Areal.Elevation;

            newZone.LeafDryMatterReadilyDegradable = shortleaf1;
            newZone.LeafDryMatterNonReadilyDegradable = shortleaf2;
            newZone.ZoneAreaM2 = newarea;
            Zones.Add(newZone);

            // Adjust the area of the dry zone that will be split by the new water level
            if (minDryZone >= 0)
                Zones[minDryZone].ZoneAreaM2 = Zones[minDryZone].CumulativeAreaM2 - Areal.Area;

            int removedDryZones = 0;
            int minZone = Zones.Count;

            // Find the range of Dry zones that will be covered by the new Wet zone and remove them
            for (int i = (Zones.Count - 1); i >= 0 ; i--)
            {
                if (Areal.Area.GreaterOrEqual(Zones[i].CumulativeAreaM2)&&Zones[i].Dry)
                {
                    minZone = i;
                    removedDryZones++;
                }      
            }

            if (removedDryZones > 0)
                Zones.RemoveRange(minZone,removedDryZones);
        }


        /// <summary>
        /// Main method for processing Dissolved Organic Carbon.
        /// Manages the addition and deletion of 'Zones', the leaf matter that resides in them, and the leaving of DOC from that matter into the water
        /// </summary>
        protected override void ProcessDoc()
        {
            if (!_initialised)
            {
                InitialiseZones();
            }

            UpdateFloodedAreas();

            // kg/m^3 * m^3 = kg
            var existingDocMassKg = ConcentrationDoc * WorkingVolume;
            var docMilligrams = existingDocMassKg * KG_TO_MG;
            
            if (Areal.Area.LessOrEqual(0.0))
                FloodCounter = 0;

            //!the DOC dissolution rate constant is temp dependent
            //Get the first order rate constant for decay of leaf litter based on temperature. 
            Leach1 = DOC_k(WaterTemperatureEst);

            //Get maximum amount of DOC that can be leached from leaf litter based on temperature.
            DocMax = DOC_max(WaterTemperatureEst);

            Leach1NonReadily = DOC_kNonReadily(WaterTemperatureEst);
            DocMaxNonReadily = DOC_maxNonReadily(WaterTemperatureEst);
            
            DOCEnteringWater = 0;
            TotalWetLeaf = 0;
            LeachingRate = 1 - eWater.Utilities.Math.Exp(-Leach1 * Sigma);
            var leachingRateNonReadily = 1 - eWater.Utilities.Math.Exp(-Leach1NonReadily * Sigma);

            //Start at the Floodplain elevation. Get the area and Elevation
            var lowerElevation = FloodplainElevation;

            // Order the zones by elevation
            var zonesByElevation = Zones.OrderBy( z => z.ElevationM ).ToList();

            foreach (var zone in zonesByElevation)
            {
                if (Areal.Area.Less(zone.CumulativeAreaM2))
                    continue;
                
                //get the elevation at the upper extent of this zone
                var upperelevation = zone.ElevationM;
                
                // Determine how much leaf is accumulated across this Zone using the elevations.
                var leafAccumulationConstant = IntergrateElevationsForAccumulation(lowerElevation, upperelevation, LeafAccumulationConstant);
                zone.LeafAccumulation = leafAccumulationConstant;

                var totalWetleafKg = zone.ZoneAreaM2 * M2_TO_HA * (zone.LeafDryMatterReadilyDegradable + zone.LeafDryMatterNonReadilyDegradable + leafAccumulationConstant);

                // Split the total wet leaf into 'Readily Degradable' and 'Non-Readily Degradable' components. The leafAccumulationConstant is split using the existing proportions.
                var readilyDegradableProportion = 
                    zone.LeafDryMatterReadilyDegradable.SafeDivide( zone.LeafDryMatterReadilyDegradable + zone.LeafDryMatterNonReadilyDegradable );

                TotalWetLeaf += totalWetleafKg;

                //TODO - Check the conversion below. How is this converting kg->mg (*1e-6) ???

                // Use the approriate Leaching Rate for the different types of mass (i.e. readily degradable & non-readily degradable)
                var readilyDegradibleDoc = totalWetleafKg * readilyDegradableProportion * 1000 * DocMax * LeachingRate;
                var nonReadilyDegradibleDoc = totalWetleafKg * (1-readilyDegradableProportion) * 1000 * DocMaxNonReadily * leachingRateNonReadily;

                DOCEnteringWater += readilyDegradibleDoc + nonReadilyDegradibleDoc;

                zone.LeafDryMatterReadilyDegradable = Math.Max(0, zone.LeafDryMatterReadilyDegradable*(1-LeachingRate));
                zone.LeafDryMatterNonReadilyDegradable = Math.Max(0, zone.LeafDryMatterNonReadilyDegradable*(1- leachingRateNonReadily));
                
                //update for next zone
                lowerElevation = upperelevation;
            }

            docMilligrams += DOCEnteringWater;
            
            // Reset the lower elevation to start the next loop
            lowerElevation = FloodplainElevation;

            foreach (var zone in zonesByElevation)
            {
                if (Areal.Area.Less(zone.CumulativeAreaM2) && zone.Dry)
                {
                    var upperelevation = zone.ElevationM;
                    var leafAccumulationConstant = IntergrateElevationsForAccumulation(lowerElevation, upperelevation, LeafAccumulationConstant);
                    zone.LeafAccumulation = leafAccumulationConstant;

                    zone.LeafDryMatterReadilyDegradable = zone.LeafDryMatterReadilyDegradable* eWater.Utilities.Math.Exp(-LeafK1) + (leafAccumulationConstant * LeafA);

                    //set max litter accumulation to large value to no maximum limit
                    const double maxmimumNonReadilyDegradable = 285000d;
                    zone.LeafDryMatterNonReadilyDegradable = Math.Min(maxmimumNonReadilyDegradable, zone.LeafDryMatterNonReadilyDegradable* eWater.Utilities.Math.Exp(-LeafK2) + (leafAccumulationConstant * (1 - LeafA)));

                    //update for next zone
                    lowerElevation = upperelevation;
                }
            }

            ConsumedDocMilligrams = docMilligrams * DocConsumptionCoefficient(WaterTemperature) * Sigma;

            docMilligrams = Math.Max(docMilligrams - ConsumedDocMilligrams,0.0);

            DissolvedOrganicCarbonLoad = docMilligrams * MG_TO_KG;
        }


        /// <summary>
        /// Initialises the Floodplain with Zones representing the initial state of the leaf matter
        /// </summary>
        public void InitialiseZones()
        {
            Zones = new List<FloodplainData>();

            if (InitialiseWithMultipleZones)
            {
                InitialiseMultipleZones();
            }
            else
            {
                InitialiseSingleZone();
            }
            _initialised = true;
        }


        /// <summary>
        /// Initialises the Zones using the "InitialLeafDryMatter" lookup tables
        /// 
        /// The precise logic used here depends on the nature of the <see cref="Zones"/> collection
        /// </summary>
        private void InitialiseMultipleZones()
        {
            // Get the min and max heights of the storage
            var minHeight = FloodplainElevation;
            var maxHeight = Areal.MaxElevation;

            // Build a collection of all the unique points when the leaf matter changes
            // Include the Min and Max elevations as valid points
            var uniqueHeights = new List<double>
            {
                minHeight,
                maxHeight
            };

            foreach (var height in InitialLeafDryMatterReadilyDegradable.ToUnsortedArray().Select(p => p.Key))
            {
                if (height > minHeight)
                    uniqueHeights.Add(height);
            }

            foreach (var height in InitialLeafDryMatterNonReadilyDegradable.ToUnsortedArray().Select(p => p.Key))
            {
                if (height > minHeight)
                    uniqueHeights.Add(height);
            }

            // Add the current elevation
            uniqueHeights.Add(Elevation);

            // Process in increasing order
            var increasingHeights = uniqueHeights.OrderBy(h => h).ToArray();


            // Due to the odd nature of the Zones collection, we shall sort the Zones into Dry and Wet
            var increasingWetZoneArray = new List<FloodplainData>();
            var tempDryZoneArray = new List<FloodplainData>();

            var previousCumulativeArea = 0.0;

            // Create the first zone from the lowest height to the next lowest height
            var previousHeight = increasingHeights[0];

            for (var i = 1; i < increasingHeights.Length; i++)
            {
                var height = increasingHeights[i];
                // Avoid duplicates
                if (height.EqualWithTolerance(previousHeight))
                    continue;

                // Anything at or below the current storage level is considered Wet
                var isWet = height <= Elevation;

                var area = Fac * Areal.AreaForHeightLookup(height);

                // Get the initial leaf matter settings
                var leafDryMatterNonReadilyDegradable =
                    IntergrateElevationsForAccumulation(
                        previousHeight,
                        height,
                        InitialLeafDryMatterNonReadilyDegradable);

                var leafDryMatterReadilyDegradable =
                    IntergrateElevationsForAccumulation(
                        previousHeight,
                        height,
                        InitialLeafDryMatterReadilyDegradable);

                // Create the zone
                var newZone = new FloodplainData(isWet)
                {
                    CumulativeAreaM2 = area,

                    // Set the area this zone covers
                    ZoneAreaM2 = area - previousCumulativeArea,
                    ElevationM = height,
                    LeafDryMatterNonReadilyDegradable = leafDryMatterNonReadilyDegradable,
                    LeafDryMatterReadilyDegradable = leafDryMatterReadilyDegradable,
                };

                // Keep track of the last area
                previousCumulativeArea = area;
                previousHeight = height;

                // Sort the zone into wet and dry
                if (newZone.Dry)
                {
                    tempDryZoneArray.Add(newZone);
                }
                else
                {
                    increasingWetZoneArray.Add(newZone);
                }
            }

            // The Zones collection has a very specific state. 
            // This state has been carried over (presumably) from the FORTRAN implemenation.
            // The rules are...
            // 1. Dry zones are grouped together and come first in the collection
            // 2. Dry zones are ordered by DECREASING area
            // 3. Wet zones are grouped together and come last in the collection
            // 4. Wet zones are ordered by INCREASING area

            tempDryZoneArray.Reverse();
            Zones.AddRange(tempDryZoneArray);
            Zones.AddRange(increasingWetZoneArray);

            // Also set the PreviousArea to the current Area so the Zones arenconsidered "stable" on the first timestep
            PreviousArea = Areal.Area;
        }


        /// <summary>
        /// Initialises the Floodplain
        /// 
        /// TODO - This method is due to be removed once the <see cref="InitialiseMultipleZones"/> method has been tested
        /// </summary>
        private void InitialiseSingleZone()
        {
            var newZone = new FloodplainData(false);
            newZone.CumulativeAreaM2 = Fac * EffectiveMaximumArea;
            newZone.ZoneAreaM2 = newZone.CumulativeAreaM2;
            newZone.ElevationM = Areal.MaxElevation;

            newZone.LeafDryMatterNonReadilyDegradable = IntergrateElevationsForAccumulation(Elevation, newZone.ElevationM,
                InitialLeafDryMatterNonReadilyDegradable);
            newZone.LeafDryMatterReadilyDegradable =
                IntergrateElevationsForAccumulation(Elevation, newZone.ElevationM, InitialLeafDryMatterReadilyDegradable);

            Zones.Add(newZone);
        }

        private double IntergrateElevationsForAccumulation(double lowerElevation, double upperElevation, LinearPerPartFunction accumulationLookup)
        {
            var lowerLoad = accumulationLookup.f(lowerElevation);
            var upperLoad = accumulationLookup.f(upperElevation);

            var elevationPoints = accumulationLookup.Select(p => p.Key).Where(p => p > lowerElevation && p < upperElevation).ToArray();

            var loads = elevationPoints.Select(p => accumulationLookup.f(p)).ToList(); //change here, get all loads, not sum
            var areas = elevationPoints.Select(p => Areal.AreaForHeightLookup(p) * 0.0001).ToList(); // get areas as well

            loads.Insert(0, lowerLoad);
            loads.Add(upperLoad);

            areas.Insert(0, Areal.AreaForHeightLookup(lowerElevation) * 0.0001);
            areas.Add(Areal.AreaForHeightLookup(upperElevation) * 0.0001);

            var lowerElevationPoints = accumulationLookup.Where(p => p.Key < lowerElevation);
            var previousElevationPoint = lowerElevationPoints.Any() ? lowerElevationPoints.Last().Key : lowerElevation;
            var previousLoad = accumulationLookup.f(previousElevationPoint);
            var previousArea = Areal.AreaForHeightLookup(previousElevationPoint) * 0.0001;


            var totalLoad = 0d;
            var totalAreaBetween = 0d;
            // start at the second element
            for (var i = 0; i < loads.Count; i++)
            {
                double pArea;
                double pLoad;
                if (i == 0)
                {
                    pArea = previousArea;
                    pLoad = previousLoad;
                }
                else
                {
                    pArea = areas[i - 1];
                    pLoad = loads[i - 1];
                }
                var areaBetween = areas[i] - pArea;
                totalLoad += areaBetween * (loads[i] + pLoad) / 2;
                totalAreaBetween += areaBetween;
            }

            // if we're accumulating across no area then our rate is 0
            if (totalAreaBetween <= 0)
                return 0;

            return totalLoad / totalAreaBetween;
            
        }

        private void PrintZones(double deltaArea)
        {
            string logFn = "D:\\temp\\zone_stats.csv";
            bool exists = File.Exists(logFn);
            var f = File.Open(logFn,FileMode.Append);
            var sw = new StreamWriter(f);

            if (!exists)
            {
                sw.WriteLine("Date,DeltaArea,Zone,Area,NewArea,Wet,LeafDryMatterReadilyDegradable,LeafDryMatterNonReadilyDegradable");
            }

            for (int i = 0; i < Zones.Count; i++)
            {
                var z = Zones[i];
                sw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}", 
                    Last.ToShortDateString(), deltaArea, i, z.CumulativeAreaM2, z.ZoneAreaM2, z.Wet,
                    z.LeafDryMatterReadilyDegradable, z.LeafDryMatterNonReadilyDegradable);
            }

            sw.Flush();
            sw.Close();
        }

        protected override double SoilO2mg()
        {
            /*
                    If  ( subrchdata(irch,isub,5) < 42. ) Then
             soilo2 = 148162 * (1. - Exp(-0.093 * (2. ** (temperature - 20.)) * subrchdata(irch,isub,5))) * subrchdata(irch,isub,2)
        else
             soilo2 = 148162 * (1. - Exp(-0.093 * (2. ** (temperature - 20.)) * subrchdata(irch,isub,5))) * subrchdata(irch,isub,2)
                If  ( soilo2 > (148000 * subrchdata(irch,isub,2)) ) Then
                      soilo2 = (148162 + 9984000 * (1 - Exp(-0.01664 * (2 ** (temperature - 20)) * subrchdata(irch,isub,5)))) * subrchdata(irch,isub,2)
                End If
        
        endif
        soilo2 = 148162 * (1. - Exp(-0.093 * (2. ** (temperature - 20.)) * subrchdata(irch,isub,5))) * subrchdata(irch,isub,2)
           */
            return 148162*(1 - eWater.Utilities.Math.Exp(-0.093*(eWater.Utilities.Math.Pow(2, WaterTemperatureEst - 20))*FloodCounter))*Areal.Area;
        }
    }
}
