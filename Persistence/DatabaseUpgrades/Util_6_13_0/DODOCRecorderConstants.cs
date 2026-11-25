using System.Collections.Generic;
using RiverSystem.Persistence.NHibernatePersistence.DatabaseUpgrades.Util_6_13_0.Constituents;

namespace FlowMatters.Source.DODOC.Persistence.DatabaseUpgrades.Util_6_13_0
{
    public class DODOCRecorderConstants : IConstituentRecorderTypesProvider
    {
        public ConstituentRecorderTypes Types => new ConstituentRecorderTypes
        {
            ProcessingModels =
            {
                {
                    "StorageDOC", // StorageDOC
                    new Dictionary<string, string>
                    {
                        { "UnprocessedLoad", "BA382B29-855B-452A-8EBB-37AA4199016F" }, // SourceSinkModel.UnprocessedLoadRecorder
                        { "ProcessedLoad", "81142A5E-5D66-4EA6-9928-55EC2F6647F6" }, // SourceSinkModel.ProcessedLoad
                        { "UnprocessedConcentration", "13BDFF04-93A7-48E6-8CF4-776B87264166" }, // SourceSinkModel.UnprocessedConcentration
                        { "Total Constituent Output Over Model Run Period", "7B400DFA-9650-4B66-921B-53AB1D749EC3" }, // SourceSinkModel.Total_Constituent_Out
                        { "Total Constituent Inflow From Upstream Over Model Run Period", "9AD736AB-696E-4FC9-82DE-64D7EED368A3" }, // SourceSinkModel.Total_Constituent_In
                        { "Flood Plain Elevation", "2B69B527-C805-4022-AD40-2C86AC158A6A" }, // ProxyStorageSourceSinkModel.FloodplainElevation
                        { "Max Accumulation Area", "8D5BF846-CEF2-4AD8-954F-91D4A27C053F" }, // StorageDOC.MaxAccumulationArea
                        { "Fraction Degradeable", "8CB5D6D1-358D-421B-AB93-F189BB4D4324" }, // StorageDOC.LeafAccumulationConstant
                        { "Reaeration Coefficient", "100EECAC-D5DC-47D2-A219-31BE3A00467B" }, // StorageDOC.ReaerationCoefficient
                        { "Readily degradable dry litter decay rate", "78CDCA14-9B7D-4132-857A-8083EFBA3524" }, // StorageDOC.LeafK1
                        { "Non-readily degradable dry litter decay rate", "86C19A1C-3654-45DB-8BC2-FF637B325DC7" }, // StorageDOC.LeafK2
                        { "Primary Production Reaeration", "5AA1A299-EABD-4545-9803-CE908FA8263A" }, // StorageDOC.PrimaryProductionReaeration
                        { "Water Temperature", "53F8F694-51C3-45E1-8B93-76F63FF3787D" }, // StorageDOC.WaterTemperature
                        { "First Order DOC Release Rate at 20ºC - Readily", "53244F44-E67A-4B76-A761-4617E517494E" }, // StorageDOC.FirstOrderDOCReleaseRateAt20DegreeC
                        { "First Order DOC Release Rate at 20ºC - Non Readily", "B7D18E1D-F0B8-47A5-A9C2-A45EAFC9B428" }, // StorageDOC.FirstOrderDOCReleaseRateAt20DegreeCNonReadily
                        { "Max DOC Released from Litter at 20ºC - Readily", "9688423E-479F-40B9-9CD8-5A0464278E21" }, // StorageDOC.MaxDOCReleasedAt20DegreeC
                        { "Max DOC Released from Litter at 20ºC - Non Readily", "D3B81B82-34D9-4B88-8B33-6FE8086752FA" }, // StorageDOC.MaxDOCReleasedAt20DegreeCNonReadily
                        { "DOC Decomposition rate at 20ºC", "CB3B22BC-BCDA-453B-92C0-C10930FC8FBF" }, // StorageDOC.DOCDecayConstantAt20DegreeC
                        { "Water Quality Factor", "968FFF9B-66E6-4378-89CF-ED1B235474BD" }, // StorageDOC.WaterQualityFactor
                        { "Weir/Dam/Spillway Reaeration Coefficient", "F5E0BBF4-AE61-4AD3-AB1C-8254D06B487C" }, // StorageDOC.StructureRerationCoefficient
                        { "Static Head Loss", "2D97A1FC-2E2D-49AB-9F31-03FFCBFDFB2E" }, // StorageDOC.StaticHeadLoss
                        { "ZoneCount", "988EF972-3DE1-493D-8269-8BCA3D022826" }, // StorageDOC.ZoneCount
                        { "CountInundatedZones", "29BE8578-749B-4165-AD16-F7D65B5CD266" }, // StorageDOC.CountInundatedZones
                        { "CountDryZones", "142F2B86-74E3-45A1-AA07-613AE97B61CA" }, // StorageDOC.CountDryZones
                        { "LeafDryMatterReadilyDegradable", "324BAF1E-DB93-4F48-B0E1-5DB53A07B333" }, // StorageDOC.LeafDryMatterReadilyDegradable
                        { "LeafDryMatterNonReadilyDegradable", "331853C9-B67E-4B40-84BD-4BC992F0E4E9" }, // StorageDOC.LeafDryMatterNonReadilyDegradable
                        { "TemperatureEst", "9FAC9C9D-CF77-4345-B1C7-B5FFBAB18F77" }, // StorageDOC.TemperatureEst
                        { "SoilO2Kg", "96440E8D-B5D2-4344-8319-7D00BA950658" }, // StorageDOC.SoilO2Kg
                        { "DoCo2", "E628DE97-6BA1-46DA-8980-0BFB98551252" }, // StorageDOC.DoCo2
                        { "Production", "77D4BF06-AEC8-4C91-A8CB-AC69F035942F" }, // StorageDOC.Production
                        { "Reaeration", "3758EA61-CE31-4F92-953F-F20F9D68A21A" }, // StorageDOC.Reaeration
                        { "ConsumedDocMilligrams", "0CD00109-E2C4-4386-9C83-3074E0AAED7A" }, // StorageDOC.ConsumedDocMilligrams
                        { "DOCEnteringWater", "56076782-4E83-4261-83D4-39204B43BB7B" }, // StorageDOC.DOCEnteringWater
                        { "TotalWetLeaf", "E707ADDB-117F-426A-8C8A-9A0E255E5DE8" }, // StorageDOC.TotalWetLeaf
                        { "LeafWetMatterReadilyDegradable", "96686574-0CDF-4A99-B426-B699667077E7" }, // StorageDOC.LeafWetMatterReadilyDegradable
                        { "LeafDryMatterReadilyDegradableRate", "D4BE53F6-5422-4842-BB76-C62228CA75C7" }, // StorageDOC.LeafDryMatterReadilyDegradableRate
                        { "LeafDryMatterNonReadilyDegradableRate", "D9D8CC09-E676-4D00-B14E-447ADC5BDE0E" }, // StorageDOC.LeafDryMatterNonReadilyDegradableRate
                        { "TotalDryMattergm2", "FF606F36-EFD2-4724-99C3-63B6EF1D9EF9" }, // StorageDOC.TotalDryMattergm2
                        { "LeafWetMatterNonReadilyDegradable", "DD06675D-E6A0-4243-9A07-79373E6EC066" }, // StorageDOC.LeafWetMatterNonReadilyDegradable
                        { "FloodplainWetAreaHa", "B6265F2B-D3B6-4C11-8533-CEE213ABBDDC" }, // StorageDOC.FloodplainWetAreaHa
                        { "FloodplainDryAreaHa", "3D91E219-6E9A-4EFE-ADE9-49CFA466CDF7" }, // StorageDOC.FloodplainDryAreaHa
                        { "Leach1", "3B7CA5D3-2C24-4E1E-9983-6704C039841E" }, // StorageDOC.Leach1
                        { "Leach1NonReadily", "7B026637-AFA5-4E1A-A4B6-29DA15F1A238" }, // StorageDOC.Leach1NonReadily
                        { "LeachingRate", "5D86324D-0E3D-4FD0-B6F3-B30F46619A56" }, // StorageDOC.LeachingRate
                        { "DocMax", "2606813A-6CB9-4DCD-8B84-A7CFFD6C9EE4" }, // StorageDOC.DocMax
                        { "DocMaxNonReadily", "D1F5BE93-71D5-4683-AAF4-C352A25CD7A4" }, // StorageDOC.DocMaxNonReadily
                        { "Total Zone Leaf Accumlation", "EEACA0C6-7F6C-4D9E-BE7F-E0932B236FE6" }, // StorageDOC.LeafAccumulation
                        { "Average Zone Leaf Accumlation", "521BD453-5E94-4F7E-8CF0-E81DB73D93DC" }, // StorageDOC.AverageLeafAccumulation
                    }
                },
                {
                    "StorageDO", // StorageDO
                    new Dictionary<string, string>
                    {
                        { "UnprocessedLoad", "BA382B29-855B-452A-8EBB-37AA4199016F" }, // SourceSinkModel.UnprocessedLoadRecorder
                        { "ProcessedLoad", "81142A5E-5D66-4EA6-9928-55EC2F6647F6" }, // SourceSinkModel.ProcessedLoad
                        { "UnprocessedConcentration", "13BDFF04-93A7-48E6-8CF4-776B87264166" }, // SourceSinkModel.UnprocessedConcentration
                        { "Total Constituent Output Over Model Run Period", "7B400DFA-9650-4B66-921B-53AB1D749EC3" }, // SourceSinkModel.Total_Constituent_Out
                        { "Total Constituent Inflow From Upstream Over Model Run Period", "9AD736AB-696E-4FC9-82DE-64D7EED368A3" }, // SourceSinkModel.Total_Constituent_In
                        { "Flood Plain Elevation", "2B69B527-C805-4022-AD40-2C86AC158A6A" }, // ProxyStorageSourceSinkModel.FloodplainElevation
                    }
                },
            }
        };
    }
}
