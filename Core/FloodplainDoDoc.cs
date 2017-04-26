using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RiverSystem;
using TIME.ManagedExtensions;

namespace FlowMatters.Source.DODOC.Core
{
    class FloodplainDoDoc : DoDocModel
    {
        public readonly double[] tempX = new[] {0d, 5d, 10d, 15d, 20d, 25d, 30d};
        public readonly double[] DOC_k = new[] {0.0, 0.0000044, 0.0000047, 0.0000049, 0.0000055, 0.0000083, 0.00001};
        public readonly double[] DOC_max = new[] {0d, 100d, 105d, 110d, 115d, 120d, 150d};
        public List<FloodplainData> Zones { get; private set;  }
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
                return Zones.Sum(z=>z.Mass(z.LeafDryMatterReadilyDegradable));
            }
        }

        public override double LeafDryMatterNonReadilyDegradable
        {
            get
            {
                return Zones.Sum(z => z.Mass(z.LeafDryMatterNonReadilyDegradable));
            }
        }

        public int FloodCounter { get; set; }

        public FloodplainDoDoc()
        {
            Zones = new List<FloodplainData>();
        }

        private void GroupDryZones()
        {
            if (Zones.Count <= 1)
                return;

            for (int i = 0; i < (Zones.Count-1); i++)
            {
                if (Zones[i].Dry && Zones[i + 1].Wet)
                {
                    for (int j = i; j < (Zones.Count - 1); j++)
                    {
                        if (Zones[j].Wet && Zones[j + 1].Dry)
                        {
                            FloodplainData tmp = Zones[j + 1];
                            Zones.RemoveAt(j + 1);
                            Zones.Insert(i+1,tmp);
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
                if (Zones[i].Area < 1)
                {
                    Zones.RemoveAt(i);
                    i = 0;
                }
            }
        }

        private void UpdateFloodedAreas()
        {
            double deltaArea = Areal.Area - PreviousArea;
            PreviousArea = Areal.Area;

            if (WorkingVolume.Less(0.0)) // Storage vs Flood storage???
            {
                deltaArea = 0.0; // +++TODO Hack hack hack...
            }

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
                if (Areal.Area < zone.Area && zone.Wet)
                {
                    if (i == (Zones.Count - 1))
                    {
                        var newDryZone = new FloodplainData();
                        newDryZone.Area = zone.Area;
                        newDryZone.LeafDryMatterReadilyDegradable = zone.LeafDryMatterReadilyDegradable;
                        newDryZone.LeafDryMatterNonReadilyDegradable = zone.LeafDryMatterNonReadilyDegradable;
                        newDryZone.NewArea = zone.Area;
                        newDryZone.Wet = false;
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
                                shortleaf1 += floodZone.LeafDryMatterReadilyDegradable*(floodZone.Area - Areal.Area)/
                                              Zones[lastFloodZone].Area;
                                shortleaf2 += floodZone.LeafDryMatterNonReadilyDegradable * (floodZone.Area - Areal.Area) /
                                              Zones[lastFloodZone].Area;
                            }
                        }
                        shortleaf1 += zone.LeafDryMatterReadilyDegradable*(zone.Area - Areal.Area)/
                                      Zones[lastFloodZone].Area;
                        shortleaf2 += zone.LeafDryMatterNonReadilyDegradable* (zone.Area - Areal.Area) /
                                      Zones[lastFloodZone].Area;

                        var newDryZone = new FloodplainData();
                        newDryZone.Area = Zones[lastFloodZone].Area;
                        newDryZone.LeafDryMatterReadilyDegradable = shortleaf1;
                        newDryZone.LeafDryMatterNonReadilyDegradable = shortleaf2;
                        newDryZone.NewArea = newDryZone.Area;
                        newDryZone.Wet = false;
                        break;
                    }
                }
            }

            /* Marker 150 */
            if(Zones.Count > 1)
            {
                for (int i = 0; i < Zones.Count; i++)
                {
                    if (Zones[i].Wet || Zones[i].Area < 1)
                    {
                        Zones.RemoveAt(i);
                        i--;
                    }
                }
            }

            for (int i = 0; i < (Zones.Count - 1); i++)
            {
                if (!(Zones[i].Area - Zones[i + 1].Area).EqualWithTolerance(Zones[i].NewArea))
                { // Why the if?
                    Zones[i].NewArea = Zones[i].Area - Zones[i + 1].Area;
                }
            }

            if (!Zones.Last().Area.EqualWithTolerance(Zones.Last().NewArea))
            { // Why the if?
                Zones.Last().NewArea = Zones.Last().Area;
            }

            FloodCounter = 0;
        }

        private void StableFloodZones()
        {
            FloodCounter++;
        }

        private void ContractFloodedZones(double reducedArea)
        {
            FloodCounter++;

            for (int i = 0; i < Zones.Count; i++)
            {
                var zone = Zones[i];
                if (Areal.Area.GreaterOrEqual(zone.Area) || zone.Dry)
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
                        double ratio = zoneJ.NewArea/reducedArea;
                        shortleaf1 += (zoneJ.LeafDryMatterReadilyDegradable*ratio);
                        shortleaf2 += (zoneJ.LeafDryMatterNonReadilyDegradable*ratio);
                    }
                }
                double ratioReducedArea = (zone.Area - Areal.Area)/reducedArea;
                shortleaf1 += (zone.LeafDryMatterReadilyDegradable*ratioReducedArea);
                    // !use remaining area
                shortleaf2 += (zone.LeafDryMatterNonReadilyDegradable*ratioReducedArea);

                var newZone = new FloodplainData();
                newZone.Area = Zones[lastFloodZone].Area;
                newZone.LeafDryMatterReadilyDegradable = shortleaf1;
                newZone.LeafDryMatterNonReadilyDegradable = shortleaf2;
                newZone.NewArea = reducedArea;
                newZone.Wet = false;
                Zones.Add(newZone);

                zone.NewArea -= zone.Area - Areal.Area;
                zone.Area = Areal.Area;
                if (zone.NewArea.Less(0.0))
                    zone.NewArea = Areal.Area;

                if (Areal.Area.EqualWithTolerance(0.0))
                    Zones.RemoveRange(i, removeWetZones);
                else
                    Zones.RemoveRange(i + 1, removeWetZones);
                return;
            }

            if (Areal.Area.Less(Zones.Last().Area) && Zones.Last().Wet)
            {
                var last = Zones.Last();

                var newZone = new FloodplainData();
                newZone.Area = last.Area;
                newZone.LeafDryMatterReadilyDegradable = last.LeafDryMatterReadilyDegradable;
                newZone.LeafDryMatterNonReadilyDegradable = last.LeafDryMatterNonReadilyDegradable;
                newZone.NewArea = reducedArea;
                newZone.Wet = false;
                Zones.Add(newZone);

                last.NewArea -= last.Area - Areal.Area;
                last.Area = Areal.Area;
                if (last.NewArea.Less(0.0))
                {
                    last.NewArea = Areal.Area;
                } // TODO ++ Common code...
            }
        }

        private void IncreaseFloodedZones(double newarea)
        {
            FloodCounter++;
            int minDryZone=-1;
            double shortleaf1, shortleaf2;
            if (Zones.Count == 1)
            {
                shortleaf1 = Zones[0].LeafDryMatterReadilyDegradable;
                shortleaf2 = Zones[0].LeafDryMatterNonReadilyDegradable;
            }
            else
            {
                minDryZone = Zones.Count-1;
                for (int i = (Zones.Count - 1); i >= 0; i--)
                {
                    if (Areal.Area.Less(Zones[i].Area) && Zones[i].Dry)
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

                        double ratioNewArea = (Zones[i].Area - (Areal.Area - newarea))/newarea;
                        shortleaf1 = shortleaf1 +
                                     Zones[i].LeafDryMatterReadilyDegradable*ratioNewArea; // !use ratio;
                        shortleaf2 = shortleaf2 +
                                     Zones[i].LeafDryMatterNonReadilyDegradable*ratioNewArea;
                        for (int j = (i - 1); j >= (minDryZone + 1); j--)
                        {
                            if (Zones[j].Dry)
                            {
                                double ratio2 = Zones[j].NewArea/newarea;
                                shortleaf1 = shortleaf1 +
                                             (Zones[j].LeafDryMatterReadilyDegradable*ratio2);
                                shortleaf2 = shortleaf2 +
                                             (Zones[j].LeafDryMatterNonReadilyDegradable*ratio2);
                            }
                        }
                        double anotherRatio = (Areal.Area - Zones[minDryZone + 1].Area)/newarea;
                        shortleaf1 = shortleaf1 + Zones[minDryZone].LeafDryMatterReadilyDegradable*anotherRatio; //  !use ratio
                        shortleaf2 = shortleaf2 + Zones[minDryZone].LeafDryMatterNonReadilyDegradable*anotherRatio;
                        break;
                    }
                }
            }

            var newZone = new FloodplainData();
            newZone.Area = Areal.Area;
            newZone.LeafDryMatterReadilyDegradable = shortleaf1;
            newZone.LeafDryMatterNonReadilyDegradable = shortleaf2;
            newZone.NewArea = newarea;
            newZone.Wet = true;
            Zones.Add(newZone);

            if (minDryZone >= 0)
                Zones[minDryZone].NewArea = Zones[minDryZone].Area - Areal.Area;

            int removedDryZones = 0;
            int minZone = Zones.Count;

            for (int i = (Zones.Count - 1); i >= 0 ; i--)
            {
                if (Areal.Area.GreaterOrEqual(Zones[i].Area)&&Zones[i].Dry)
                {
                    minZone = i;
                    removedDryZones++;
                }      
            }

            if (removedDryZones > 0)
                Zones.RemoveRange(minZone,removedDryZones);
        }

        protected override void ProcessDoc()
        {
            if (Zones.Count == 0)
            {
                var newZone = new FloodplainData();
                newZone.Area = Fac*MaxAccumulationArea;
                newZone.LeafDryMatterNonReadilyDegradable = InitialLeafDryMatterNonReadilyDegradable;
                newZone.LeafDryMatterReadilyDegradable = InitialLeafDryMatterReadilyDegradable;
                Zones.Add(newZone);
            }

            UpdateFloodedAreas();

            var doc = 0.0;

            if (Areal.Area.LessOrEqual(0.0))
                FloodCounter = 0;


            //!the DOC dissolution rate constant is temp dependent
            double leach1 = AbstractLumpedFlowRouting.Lintrpl(tempX.ToList(), DOC_k.ToList(), TemperatureEst,
                DOC_k.Length);
            double DOCmax = AbstractLumpedFlowRouting.Lintrpl(tempX.ToList(), DOC_max.ToList(), TemperatureEst,
                DOC_max.Length);

            var decomp1 = DecompositionCoefficient*leach1;

            foreach (var zone in Zones)
            {
                if (Areal.Area.Less(zone.Area))
                    continue;

                var scale = 1.0;
                if (Areal.Area < (Fac*MaxAccumulationArea/10))
                {
                    scale = 1.3;
                }
                double wetleaf = zone.NewArea*
                                 (zone.LeafDryMatterReadilyDegradable + zone.LeafDryMatterNonReadilyDegradable)*
                                 scale + zone.NewArea*LeafAccumulationConstant;

                double leafDOC = wetleaf*1000*DOCmax*(1 - Math.Exp(-leach1*Sigma*86400));
                doc += leafDOC;

                /*
                  zone(floodrch,isub,ii,2) = max(0.,zone(floodrch,isub,ii,2)-(zone(floodrch,isub,ii,2) * (1 - Exp(-decomp1 * sigma *  86400))))
                  zone(floodrch,isub,ii,3) = max(0.,zone(floodrch,isub,ii,3)-(zone(floodrch,isub,ii,3) * (1 - Exp(-decomp1 * sigma *  86400))))
                */
                double leadingRate = Math.Exp(-decomp1*Sigma*86400);
                zone.LeafDryMatterReadilyDegradable = Math.Max(0, zone.LeafDryMatterReadilyDegradable*leadingRate);
                zone.LeafDryMatterNonReadilyDegradable = Math.Max(0, zone.LeafDryMatterNonReadilyDegradable*leadingRate);

            }

            foreach (var zone in Zones)
            {
                if (Areal.Area.Less(zone.Area) && zone.Dry)
                {
                    zone.LeafDryMatterReadilyDegradable = zone.LeafDryMatterReadilyDegradable*Math.Exp(-LeafK1) +
                                                          (LeafAccumulationConstant*LeafA);
                    zone.LeafDryMatterNonReadilyDegradable = Math.Min(2850d,
                        zone.LeafDryMatterNonReadilyDegradable*Math.Exp(-LeafK2) +
                        (LeafAccumulationConstant*(1 - LeafA)));
                }
            }

            ConsumedDoc = ConcentrationDoc*WorkingVolume*DocConsumptionCoefficient*Sigma*1e6;
            ConsumedDoc += DocConsumptionCoefficient*doc*Sigma;

            doc = (doc - ConsumedDoc)*1e-6;

            DissolvedOrganicCarbonLoad = doc;


/*
                                                ! consumption of DOC by microbial activity (temp dependent)
                                    
                                                Rchlod(Irch,2)=1.
                                            Valcon(Ircvld(Irch,WQno))=subloadDOC/fac
                                            DOCcalc(irch,isub,1)=1.
                                            if ( subrchdata(irch,isub,4).le.zerost ) then 
                                                Valcon(Ircvld(Irch,WQno))=0.
                                            endif  
                                            continue
                                    
                                                                                    */

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
                    Last.ToShortDateString(), deltaArea, i, z.Area, z.NewArea, z.Wet,
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
            return 148162*(1 - Math.Exp(-0.093*(Math.Pow(2, TemperatureEst - 20))*FloodCounter))*Areal.Area;
        }
    }

    public class FloodplainData
    {
        public double M2_TO_HA = 1e-4;
        public double Area { get; set; }
        public double LeafDryMatterReadilyDegradable { get; set; } // mass/ha
        public double LeafDryMatterNonReadilyDegradable { get; set; } // mass/ha
        public double NewArea { get; set; }
        public bool Wet { get; set; }
        public bool Dry { get { return !Wet; } }

        internal double Mass(double byArea)
        {
            return Wet ? 0.0 : (NewArea*M2_TO_HA)*byArea;
        }
    }
}
