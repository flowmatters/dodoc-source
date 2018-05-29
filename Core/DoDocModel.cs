using System;
using TIME.Core;
using TIME.Core.Metadata;
using TIME.ManagedExtensions;
using TIME.Science.Mathematics.Functions;

namespace FlowMatters.Source.DODOC.Core
{
    public abstract class DoDocModel
    {
        protected DoDocModel()
        {
            InitialLeafDryMatterNonReadilyDegradable = new LinearPerPartFunction();
            InitialLeafDryMatterReadilyDegradable = new LinearPerPartFunction();
        }
        
        private const double MG_L_to_KG_M3 = 1e-3;
        private const double KG_M3_to_MG_L = 1e3;
        protected const double MG_TO_KG = 1e-6;
        protected const double KG_TO_MG = 1e6;
        private const double M3_to_L = 1e3;
        protected const double M2_TO_HA = 1e-4;

        public double WorkingVolume
        {
            get; set;
        }

        public virtual int ZoneCount => 0;
        public virtual int CountInundatedZones => 0;
        public virtual int CountDryZones => 0;

        public virtual double LeafDryMatterReadilyDegradable => 0;
        public virtual double LeafDryMatterNonReadilyDegradable => 0;

        public virtual double LeafWetMatterReadilyDegradable => 0;
        public virtual double LeafWetMatterNonReadilyDegradable => 0;

        public virtual double FloodplainDryAreaHa => 0;
        public virtual double FloodplainWetAreaHa => 0;
        
        // public double[] tempX { get; set; } = {0d, 5d, 10d, 15d, 20d, 25d, 30d};
        
        /// <summary>
        /// DOC-k is the first order rate constant for decay of leaf litter. 
        /// </summary>
        /// <param name="tempDegreeC"></param>
        /// <returns></returns>
        public double DOC_k(double tempDegreeC)
        {
            //NOTE: Previously a hardcoded array => { get; set; } = {0.0, 0.38016, 0.40608, 0.42336, 0.4752, 0.71712, 0.864};
           
            var DOC_k20 = FirstOrderDOCReleaseRateAt20DegreeC;

            //DOC_k = DOC_k20C * 1.03(T-20)        ### with default DOC_k20C = 0.864 for leaves (as per below).
            //   where DOC_k20C => is the DOC-k at 20 degrees. e.g. DOC_k20C for leaves is said to be 0.864 
            return DOC_k20 * 1.03 * (tempDegreeC - 20);
        }
        
        /// <summary>
        /// DOC_max = maximum amount of DOC that can be leached from leaf litter
        /// </summary>
        /// <param name="tempDegreeC"></param>
        /// <returns></returns>
        public double DOC_max(double tempDegreeC)
        {
            //NOTE: Previously a hardcoded array => { get; set; } = {0d, 100d, 105d, 110d, 115d, 120d, 150d};
            
            var m20 = MaxDOCReleasedAt20DegreeC;

            // m = m20 x (0.93255 + 0.06745 x e^(0.2047 * (T - 20)))
            //   where m20 => is the value of m measured at 20 degree C
            //   where T   => is the temperature in degree C
            //   returns m => maximum amount of DOC released (mg g^-1)
            return m20 * (0.93255 + 0.06745 * Math.Pow(Math.E, 0.2047 * (tempDegreeC - 20)));
        }

        /// <summary>
        /// Temperature sensitive decay constant for DOC 
        /// NOTE: This won't return DOCDecayConstantAt20DegreeC at temperatureObs = 20 due to rounding in the function
        ///       this has been implemented as suggested in Whitworth and Baldwin 202016 BRAT
        /// </summary>
        /// <param name="waterTemperature"></param>
        /// <returns></returns>
        public double DocConsumptionCoefficient(double waterTemperature)
        {
            return DOCDecayConstantAt20DegreeC * (-0.2088 + (0.0604 * waterTemperature));
        }
        
        public double[] ProductionCoefficients { get; set; } = {1.0, 0.75, 0.50, 0.25, 0.1};
        public double[] ProductionBreaks { get; set; } = {3d, 5d, 8d, 20d};

        [Parameter]
        // +++TODO Fortran says - mg.L.day - does that mean mg/L/day? g/kl, kg/ML
        public double PrimaryProductionReaeration { get; set; } = 0.43; 

        public IAreal Areal { get; set; }

        [Input] public double WaterTemperature { get; set; }
        [Output] public double WaterTemperatureEst { get; protected set; }
        protected double Sigma;

        /// <summary>
        /// Maximum amount of DOC that can be leached from leaf litter at 20 degree C
        /// </summary>
        [Parameter]
        public double MaxDOCReleasedAt20DegreeC { get; set; }
        
        /// <summary>
        /// First order rate constant for decay of leaf / bark / trig litter at 20 degree C
        /// </summary>
        [Parameter]
        public double FirstOrderDOCReleaseRateAt20DegreeC { get; set; }

        [Parameter]
        public double DOCDecayConstantAt20DegreeC { get; set; }

        public bool Debug { get; set; }

        [Output] 
        public double ConsumedDocMilligrams { get; set; }
        [Output]
        public double DissolvedOrganicCarbonLoad { get; set; }
        [Output]
        public double DissolvedOxygenLoad { get; set; }

        [Parameter]
        public double SoilO2Scaling { get; set; }

        [Parameter]
        public double Fac { get; set; }

        #region Parameters from valcon
        [Parameter]
        public LinearPerPartFunction LeafA { get; set; }
        [Parameter]
        public double LeafK1 { get; set; }
        [Parameter]
        public double LeafK2 { get; set; }
        [Parameter]
        public double LeafAccumulationConstant { get; set; }
        [Parameter]
        public double ReaerationCoefficient { get; set; }

        [Parameter]
        public double WaterQualityFactor { get; set; }

        [Parameter]
        public double StructureRerationCoefficient { get; set; }

        [Parameter, CalculationUnits(CommonUnits.metres)]
        public double StaticHeadLoss { get; set; }

        #endregion

        [Input]
        public double ConcentrationDoc { get; set; }
        [Input]
        public double ConcentrationDo { get; set; }

        [Parameter]
        public double MaxAccumulationArea { get; set; }

        public double EffectiveMaximumArea
        {
            get
            {
                if (MaxAccumulationArea > 0)
                {
                    return Math.Min(MaxAccumulationArea, Areal.MaxArea);
                }
                return Areal.MaxArea;
            }
        }
        
        public LinearPerPartFunction InitialLeafDryMatterReadilyDegradable { get; set; }
        public LinearPerPartFunction InitialLeafDryMatterNonReadilyDegradable { get; set; }

        protected DateTime Last;
        [Output]
        public double SoilO2Kg { get; private set; }
        [Output]
        public double DoCo2 { get; private set; }
        [Output]
        public double Production { get; private set; }
        [Output]
        public double Reaeration { get; private set; }

        [Output]
        public double DOCEnteringWater { get; protected set; }

        [Output]
        public double TotalWetLeaf { get; protected set; }

        [Output]
        public double LeachingRate { get; protected set; }

        [Output]
        public double Leach1 { get; protected set; }

        [Output]
        public double DocMax { get; protected set; }

        [Output]
        public double LeafAccumulation => LeafA.f(Elevation);

        public double Elevation { get; set; }

        public void Run(DateTime dt)
        {
            if (dt.Date == Last.Date)
                return;

            Last = dt;

            PreTimeStep(dt);
            ProcessDoc();

            ProcessDo();
        }

        private void PreTimeStep(DateTime dt)
        {
            // +++TODO How many of these should be parameters to make the model transferable???
            if (WaterTemperature > 0)
                WaterTemperatureEst = WaterTemperature;
            else
                //Default: Use sin curve to predict what the temprature at supplied date.
                WaterTemperatureEst = 17.2388 + (7.8574 * Math.Sin(((2 * Math.PI * dt.DayOfYear) / 361.8) + 1.178));

            Sigma = Math.Pow(1.05, WaterTemperatureEst - 20);
        }

        protected abstract void ProcessDoc();
        protected abstract double SoilO2mg();

        private void ProcessDo()
        {
            // Bring existing concentration into it?
             
            SoilO2Kg = SoilO2Scaling * 1e-6 * SoilO2mg();

            DoCo2 = 1e-6 * ConsumedDocMilligrams * 2.667;

            var saturatedo2mg_L = 13.41 * Math.Exp(-0.01905 * WaterTemperatureEst); // +++TODO CONFIRM UNITS????
            var waterColumnConcentrationDOKg_M3 = Math.Min(ConcentrationDo, saturatedo2mg_L * MG_L_to_KG_M3);
            var existingWaterColumnDOKg = WorkingVolume * waterColumnConcentrationDOKg_M3;
            var waterColumnConcentrationDOmg_L = waterColumnConcentrationDOKg_M3 * KG_M3_to_MG_L;

            // +++TODO Check unit conversions!
            var reaerationmg = ReaerationCoefficient * Math.Max(0, (saturatedo2mg_L /*mg.L-1*/ - waterColumnConcentrationDOmg_L)) * WorkingVolume * M3_to_L;
            Reaeration = reaerationmg*MG_TO_KG;

            int i;
            var concentrationDOCmgL = ConcentrationDoc*KG_M3_to_MG_L;
            for (i = 0; i < ProductionBreaks.Length; i++)
            {
                // +++TODO Should this concentration be the updated concentration, post DOC run?
                if (concentrationDOCmgL <= ProductionBreaks[i])
                    break;
            }
            Production = (PrimaryProductionReaeration * MG_L_to_KG_M3) * WorkingVolume * ProductionCoefficients[i];

            var totalOxygenUnconstrainedKg = (existingWaterColumnDOKg + Production + Reaeration) - (SoilO2Kg + DoCo2);

            var saturationOxygenKg = saturatedo2mg_L * MG_L_to_KG_M3 * WorkingVolume;

            // extra DO from generated from regulated scructures
            var deficitRatio = 1 + 0.38 * StructureRerationCoefficient * WaterQualityFactor * StaticHeadLoss * (1 - 0.11 * StaticHeadLoss) * (1 + 0.046 * WaterTemperature);
            var doFromRegulatedScructureKg = saturationOxygenKg - (saturationOxygenKg - totalOxygenUnconstrainedKg) / deficitRatio;

            var totalOxygen = Math.Min(doFromRegulatedScructureKg, saturationOxygenKg);
            var subloadO2 = totalOxygen / Fac;

            if (WorkingVolume.Greater(0.0))
                DissolvedOxygenLoad = Math.Max(subloadO2, 0.0);
            else
                DissolvedOxygenLoad = 0.0;

        }

    }
}
 