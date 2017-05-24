using System;
using RiverSystem;
using TIME.Core;
using TIME.Core.Metadata;
using TIME.ManagedExtensions;

namespace FlowMatters.Source.DODOC.Core
{
    public abstract class DoDocModel
    {
        public const double MG_L_to_KG_M3 = 1e-3;
        public const double KG_M3_to_MG_L = 1e3;
        public const double MG_TO_KG = 1e-6;
        public const double KG_TO_MG = 1e6;
        public const double M3_to_L = 1e3;
        public const double M2_TO_HA = 1e-4;

        public double WorkingVolume
        {
            get;set;
        }

        public virtual int ZoneCount { get { return 0; } }
        public virtual int CountInundatedZones { get { return 0; } }
        public virtual int CountDryZones { get { return 0; } }

        public virtual double LeafDryMatterReadilyDegradable { get { return 0; } }
        public virtual double LeafDryMatterNonReadilyDegradable { get { return 0; } }

        public readonly double[] ProductionCoefficients = new[] {1.0, 0.75, 0.50, 0.25, 0.1 };
        public readonly int[] ProductionBreaks = new[] {3, 5, 8, 20};

        [Parameter]
        public double PrimaryProductionReaeration { get; set; } = 0.43; // +++TODO Fortran says - mg.L.day - does that mean mg/L/day? g/kl, kg/ML

        public Areal Areal { get; set; }

        [Output] public double TemperatureEst { get; protected set; }
        protected double Sigma;

        public bool Debug { get; set; }

        [Output] public double ConsumedDocMilligrams { get; set; }
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
        public double DecompositionCoefficient { get; set; }

        [Parameter]
        public double DocConsumptionCoefficient { get; set; }

        [Parameter]
        public double LeafA { get; set; }
        [Parameter]
        public double LeafK1 { get; set; }
        [Parameter]
        public double LeafK2 { get; set; }
        [Parameter]
        public double LeafAccumulationConstant { get; set; }
        [Parameter]
        public double ReaerationCoefficient { get; set; }        
        #endregion

        [Input]
        public double ConcentrationDoc { get; set; }
        [Input]
        public double ConcentrationDo { get; set; }

        [Parameter]
        public double MaxAccumulationArea { get; set; }

        [Parameter, CalculationUnits("kg.ha^-1")]
        public double InitialLeafDryMatterReadilyDegradable { get; set; }

        [Parameter,CalculationUnits("kg.ha^-1")]
        public double InitialLeafDryMatterNonReadilyDegradable { get; set; }

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

        public void Run(DateTime dt)
        {
            if (dt.Date == Last.Date)
                return;

            if (dt.Date < Last.Date)
                Reset();

            Last = dt;

            PreTimeStep(dt);
            ProcessDoc();

            ProcessDo();
        }

        protected virtual void Reset()
        {
        }

        protected virtual void PreTimeStep(DateTime dt)
        {
            // +++TODO How many of these should be parameters to make the model transferable???
            TemperatureEst = 17.2388 + (7.8574 * Math.Sin(((2 * Math.PI * dt.DayOfYear) / 361.8) + 1.178));
            Sigma = Math.Pow(1.05, TemperatureEst - 20);

            // Reaeration_coeff = valcon(ReaerCoeff(irch))       
            // DOC_consumption_coeff = valcon(DOCCoeff(irch))

        }

        protected abstract void ProcessDoc();
        protected abstract double SoilO2mg();

        protected virtual void ProcessDo()
        {
            // Bring existing concentration into it?

            SoilO2Kg = SoilO2Scaling*1e-6*SoilO2mg();

            DoCo2 = 1e-6 * ConsumedDocMilligrams/20*32;

            var saturatedo2mg_L = 13.41*Math.Exp(-0.01905*TemperatureEst); // +++TODO CONFIRM UNITS????
            var waterColumnConcentrationDOKg_M3 = Math.Min(ConcentrationDo, saturatedo2mg_L*MG_L_to_KG_M3);
            var existingWaterColumnDOKg = WorkingVolume * waterColumnConcentrationDOKg_M3;
            var waterColumnConcentrationDOmg_L = waterColumnConcentrationDOKg_M3 * KG_M3_to_MG_L;

            // +++TODO Check unit conversions!
            var reaerationmg = ReaerationCoefficient* Math.Max(0, (saturatedo2mg_L/*mg.L-1*/ - waterColumnConcentrationDOmg_L))* WorkingVolume * M3_to_L;
            Reaeration = reaerationmg*MG_TO_KG;

            int i;
            var concentrationDOCmgL = ConcentrationDoc*KG_M3_to_MG_L;
            for (i = 0; i < ProductionBreaks.Length; i++)
            {
                // +++TODO Should this concentration be the updated concentration, post DOC run?
                if (concentrationDOCmgL <= ProductionBreaks[i])
                    break;
            }
            Production = (PrimaryProductionReaeration*MG_L_to_KG_M3)*WorkingVolume*ProductionCoefficients[i];

            var totalOxygenUnconstrainedKg = (existingWaterColumnDOKg + Production + Reaeration) - (SoilO2Kg + DoCo2);
            var saturationOxygenKg = saturatedo2mg_L*MG_L_to_KG_M3*WorkingVolume;
            var totalOxygen = Math.Min(totalOxygenUnconstrainedKg, saturationOxygenKg);
            var subloadO2 = totalOxygen/Fac;

            if (WorkingVolume.Greater(0.0))
                DissolvedOxygenLoad = Math.Max(subloadO2,0.0);
            else
                DissolvedOxygenLoad = 0.0;
        }

    }
}
 