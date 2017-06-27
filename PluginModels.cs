

using RiverSystem;
using TIME.Core.Metadata;
using TIME.Core;

namespace FlowMatters.Source.DODOC.Instream
{
    public class InstreamDO : ProxyLinkSourceSinkModel
    {
        public InstreamDO()
        {
        }

					public override LinkSourceSinkModel CloneForMultipleDivisions()
			{
				return new InstreamDO() { 
									IsFloodplain = IsFloodplain,
				};
							}
		
		
        protected override void UpdateWorker(double constituentConcentration)
        {
            Worker.ConcentrationDo = constituentConcentration;
        }

        protected override void RetrieveResults()
        {
            ProcessedLoad = Worker.DissolvedOxygenLoad;
        }
    }

    public class InstreamDOC : ProxyLinkSourceSinkModel
    {
        public InstreamDOC()
        {
        }

        // WHEN ADDING PROPERTIES, REMEMBER TO CLONE!
		[Parameter,CalculationUnits(CommonUnits.squareMetres)] public double MaxAccumulationArea {get; set;}
		[Parameter] public double LeafAccumulationConstant {get; set;}
		[Parameter] public double ReaerationCoefficient {get; set;}
		[Parameter] public double DocConsumptionCoefficient {get; set;}
		[Parameter] public double LeafA {get; set;}
		[Parameter] public double LeafK1 {get; set;}
		[Parameter] public double LeafK2 {get; set;}
		[Parameter] public double InitialLeafDryMatterReadilyDegradable {get; set;}
		[Parameter] public double InitialLeafDryMatterNonReadilyDegradable {get; set;}
		[Parameter] public double PrimaryProductionReaeration {get; set;}
		[Parameter] public double TemperatureObs {get; set;}
		
		[Parameter] public double[] tempX {get; set;}
		[Parameter] public double[] DOC_max {get; set;}
		[Parameter] public double[] DOC_k {get; set;}
		[Parameter] public double[] ProductionCoefficients {get; set;}
		[Parameter] public double[] ProductionBreaks {get; set;}
				
		[Output] public double ZoneCount {get; private set;}

		[Output] public int CountInundatedZones {get; private set;}

		[Output] public int CountDryZones {get; private set;}

		[Output] public double LeafDryMatterReadilyDegradable {get; private set;}

		[Output] public double LeafDryMatterNonReadilyDegradable {get; private set;}

		[Output] public double TemperatureEst {get; private set;}

		[Output] public double SoilO2Kg {get; private set;}

		[Output] public double DoCo2 {get; private set;}

		[Output] public double Production {get; private set;}

		[Output] public double Reaeration {get; private set;}

		[Output] public double ConsumedDocMilligrams {get; private set;}

		[Output] public double DOCEnteringWater {get; private set;}

		[Output] public double TotalWetLeaf {get; private set;}

		[Output] public double LeafWetMatterReadilyDegradable {get; private set;}

		[Output] public double LeafWetMatterNonReadilyDegradable {get; private set;}

		[Output] public double FloodplainWetAreaHa {get; private set;}

		[Output] public double FloodplainDryAreaHa {get; private set;}

		[Output] public double Leach1 {get; private set;}

		[Output] public double LeachingRate {get; private set;}

		[Output] public double DocMax {get; private set;}

		
		public override LinkSourceSinkModel CloneForMultipleDivisions()
{
	return new InstreamDOC() { 
					IsFloodplain = IsFloodplain,
								 
	MaxAccumulationArea = MaxAccumulationArea,
				 
	LeafAccumulationConstant = LeafAccumulationConstant,
				 
	ReaerationCoefficient = ReaerationCoefficient,
				 
	DocConsumptionCoefficient = DocConsumptionCoefficient,
				 
	LeafA = LeafA,
				 
	LeafK1 = LeafK1,
				 
	LeafK2 = LeafK2,
				 
	InitialLeafDryMatterReadilyDegradable = InitialLeafDryMatterReadilyDegradable,
				 
	InitialLeafDryMatterNonReadilyDegradable = InitialLeafDryMatterNonReadilyDegradable,
				 
	PrimaryProductionReaeration = PrimaryProductionReaeration,
				 
	TemperatureObs = TemperatureObs,
									tempX = (double[])((tempX==null)?null:tempX.Clone()),
					DOC_max = (double[])((DOC_max==null)?null:DOC_max.Clone()),
					DOC_k = (double[])((DOC_k==null)?null:DOC_k.Clone()),
					ProductionCoefficients = (double[])((ProductionCoefficients==null)?null:ProductionCoefficients.Clone()),
					ProductionBreaks = (double[])((ProductionBreaks==null)?null:ProductionBreaks.Clone()),
					};
	}
		
		
        protected override void UpdateWorker(double constituentConcentration)
        {
            Worker.ConcentrationDoc = constituentConcentration;

						Worker.MaxAccumulationArea = MaxAccumulationArea;
						Worker.LeafAccumulationConstant = LeafAccumulationConstant;
						Worker.ReaerationCoefficient = ReaerationCoefficient;
						Worker.DocConsumptionCoefficient = DocConsumptionCoefficient;
						Worker.LeafA = LeafA;
						Worker.LeafK1 = LeafK1;
						Worker.LeafK2 = LeafK2;
						Worker.InitialLeafDryMatterReadilyDegradable = InitialLeafDryMatterReadilyDegradable;
						Worker.InitialLeafDryMatterNonReadilyDegradable = InitialLeafDryMatterNonReadilyDegradable;
						Worker.PrimaryProductionReaeration = PrimaryProductionReaeration;
						Worker.TemperatureObs = TemperatureObs;
			
						  if(tempX==null){
			tempX = Worker.tempX;
			  } else {
			Worker.tempX = tempX;
			  }
						  if(DOC_max==null){
			DOC_max = Worker.DOC_max;
			  } else {
			Worker.DOC_max = DOC_max;
			  }
						  if(DOC_k==null){
			DOC_k = Worker.DOC_k;
			  } else {
			Worker.DOC_k = DOC_k;
			  }
						  if(ProductionCoefficients==null){
			ProductionCoefficients = Worker.ProductionCoefficients;
			  } else {
			Worker.ProductionCoefficients = ProductionCoefficients;
			  }
						  if(ProductionBreaks==null){
			ProductionBreaks = Worker.ProductionBreaks;
			  } else {
			Worker.ProductionBreaks = ProductionBreaks;
			  }
			
						Worker.Fac = 1.0/(this.Link.Divisions.Count);
			        }

        protected override void RetrieveResults()
        {
            ProcessedLoad = Worker.DissolvedOrganicCarbonLoad;
						ZoneCount = Worker.ZoneCount;
						CountInundatedZones = Worker.CountInundatedZones;
						CountDryZones = Worker.CountDryZones;
						LeafDryMatterReadilyDegradable = Worker.LeafDryMatterReadilyDegradable;
						LeafDryMatterNonReadilyDegradable = Worker.LeafDryMatterNonReadilyDegradable;
						TemperatureEst = Worker.TemperatureEst;
						SoilO2Kg = Worker.SoilO2Kg;
						DoCo2 = Worker.DoCo2;
						Production = Worker.Production;
						Reaeration = Worker.Reaeration;
						ConsumedDocMilligrams = Worker.ConsumedDocMilligrams;
						DOCEnteringWater = Worker.DOCEnteringWater;
						TotalWetLeaf = Worker.TotalWetLeaf;
						LeafWetMatterReadilyDegradable = Worker.LeafWetMatterReadilyDegradable;
						LeafWetMatterNonReadilyDegradable = Worker.LeafWetMatterNonReadilyDegradable;
						FloodplainWetAreaHa = Worker.FloodplainWetAreaHa;
						FloodplainDryAreaHa = Worker.FloodplainDryAreaHa;
						Leach1 = Worker.Leach1;
						LeachingRate = Worker.LeachingRate;
						DocMax = Worker.DocMax;
			        }
    }

}

namespace FlowMatters.Source.DODOC.Storage
{
    public class StorageDO : ProxyStorageSourceSinkModel
    {
        public StorageDO()
        {
        }

		
        protected override void UpdateWorker(double constituentConcentration)
        {
            Worker.ConcentrationDo = constituentConcentration;
        }

        protected override void RetrieveResults()
        {
            ProcessedLoad = Worker.DissolvedOxygenLoad;
        }
    }

    public class StorageDOC : ProxyStorageSourceSinkModel
    {
        public StorageDOC()
        {
        }

        // WHEN ADDING PROPERTIES, REMEMBER TO CLONE!
		[Parameter,CalculationUnits(CommonUnits.squareMetres)] public double MaxAccumulationArea {get; set;}
		[Parameter] public double LeafAccumulationConstant {get; set;}
		[Parameter] public double ReaerationCoefficient {get; set;}
		[Parameter] public double DocConsumptionCoefficient {get; set;}
		[Parameter] public double LeafA {get; set;}
		[Parameter] public double LeafK1 {get; set;}
		[Parameter] public double LeafK2 {get; set;}
		[Parameter] public double InitialLeafDryMatterReadilyDegradable {get; set;}
		[Parameter] public double InitialLeafDryMatterNonReadilyDegradable {get; set;}
		[Parameter] public double PrimaryProductionReaeration {get; set;}
		[Parameter] public double TemperatureObs {get; set;}
		
		[Parameter] public double[] tempX {get; set;}
		[Parameter] public double[] DOC_max {get; set;}
		[Parameter] public double[] DOC_k {get; set;}
		[Parameter] public double[] ProductionCoefficients {get; set;}
		[Parameter] public double[] ProductionBreaks {get; set;}
				
		[Output] public double ZoneCount {get; private set;}

		[Output] public int CountInundatedZones {get; private set;}

		[Output] public int CountDryZones {get; private set;}

		[Output] public double LeafDryMatterReadilyDegradable {get; private set;}

		[Output] public double LeafDryMatterNonReadilyDegradable {get; private set;}

		[Output] public double TemperatureEst {get; private set;}

		[Output] public double SoilO2Kg {get; private set;}

		[Output] public double DoCo2 {get; private set;}

		[Output] public double Production {get; private set;}

		[Output] public double Reaeration {get; private set;}

		[Output] public double ConsumedDocMilligrams {get; private set;}

		[Output] public double DOCEnteringWater {get; private set;}

		[Output] public double TotalWetLeaf {get; private set;}

		[Output] public double LeafWetMatterReadilyDegradable {get; private set;}

		[Output] public double LeafWetMatterNonReadilyDegradable {get; private set;}

		[Output] public double FloodplainWetAreaHa {get; private set;}

		[Output] public double FloodplainDryAreaHa {get; private set;}

		[Output] public double Leach1 {get; private set;}

		[Output] public double LeachingRate {get; private set;}

		[Output] public double DocMax {get; private set;}

		
		
        protected override void UpdateWorker(double constituentConcentration)
        {
            Worker.ConcentrationDoc = constituentConcentration;

						Worker.MaxAccumulationArea = MaxAccumulationArea;
						Worker.LeafAccumulationConstant = LeafAccumulationConstant;
						Worker.ReaerationCoefficient = ReaerationCoefficient;
						Worker.DocConsumptionCoefficient = DocConsumptionCoefficient;
						Worker.LeafA = LeafA;
						Worker.LeafK1 = LeafK1;
						Worker.LeafK2 = LeafK2;
						Worker.InitialLeafDryMatterReadilyDegradable = InitialLeafDryMatterReadilyDegradable;
						Worker.InitialLeafDryMatterNonReadilyDegradable = InitialLeafDryMatterNonReadilyDegradable;
						Worker.PrimaryProductionReaeration = PrimaryProductionReaeration;
						Worker.TemperatureObs = TemperatureObs;
			
						  if(tempX==null){
			tempX = Worker.tempX;
			  } else {
			Worker.tempX = tempX;
			  }
						  if(DOC_max==null){
			DOC_max = Worker.DOC_max;
			  } else {
			Worker.DOC_max = DOC_max;
			  }
						  if(DOC_k==null){
			DOC_k = Worker.DOC_k;
			  } else {
			Worker.DOC_k = DOC_k;
			  }
						  if(ProductionCoefficients==null){
			ProductionCoefficients = Worker.ProductionCoefficients;
			  } else {
			Worker.ProductionCoefficients = ProductionCoefficients;
			  }
						  if(ProductionBreaks==null){
			ProductionBreaks = Worker.ProductionBreaks;
			  } else {
			Worker.ProductionBreaks = ProductionBreaks;
			  }
			
						Worker.Fac = 1.0;
			        }

        protected override void RetrieveResults()
        {
            ProcessedLoad = Worker.DissolvedOrganicCarbonLoad;
						ZoneCount = Worker.ZoneCount;
						CountInundatedZones = Worker.CountInundatedZones;
						CountDryZones = Worker.CountDryZones;
						LeafDryMatterReadilyDegradable = Worker.LeafDryMatterReadilyDegradable;
						LeafDryMatterNonReadilyDegradable = Worker.LeafDryMatterNonReadilyDegradable;
						TemperatureEst = Worker.TemperatureEst;
						SoilO2Kg = Worker.SoilO2Kg;
						DoCo2 = Worker.DoCo2;
						Production = Worker.Production;
						Reaeration = Worker.Reaeration;
						ConsumedDocMilligrams = Worker.ConsumedDocMilligrams;
						DOCEnteringWater = Worker.DOCEnteringWater;
						TotalWetLeaf = Worker.TotalWetLeaf;
						LeafWetMatterReadilyDegradable = Worker.LeafWetMatterReadilyDegradable;
						LeafWetMatterNonReadilyDegradable = Worker.LeafWetMatterNonReadilyDegradable;
						FloodplainWetAreaHa = Worker.FloodplainWetAreaHa;
						FloodplainDryAreaHa = Worker.FloodplainDryAreaHa;
						Leach1 = Worker.Leach1;
						LeachingRate = Worker.LeachingRate;
						DocMax = Worker.DocMax;
			        }
    }

}



