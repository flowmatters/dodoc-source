namespace FlowMatters.Source.DODOC.Core
{
    public class RoutingDoDoc : DoDocModel
    {
        protected override void ProcessDoc()
        {
            var load = ConcentrationDoc * WorkingVolume;
            ConsumedDocMilligrams = load*DocConsumptionCoefficient*Sigma*KG_TO_MG;
            //DissolvedOrganicCarbonLoad = -1* ConsumedDoc / Fac*1e-6;
            DissolvedOrganicCarbonLoad = load - ConsumedDocMilligrams*MG_TO_KG;

            /*
                            !only DOC and O2 loads calculated (remove 1% DOC per day)
                            !current DOC concentration for subreach  is    
                            consumedDOC(irch,isub)= concent(irch,isub,WQno) * subrchdata(irch,isub,4) * DOC_consumption_coeff * sigma * 1.E+6  !mg

                            subloadDOC= -1.* consumedDOC(irch,isub)/fac * 1.E-6 !convert to kg
                            Rchlod(Irch,WQno)=1.
                            Valcon(Ircvld(Irch,WQno))=subloadDOC
                            DOCcalc(irch,isub,1)=1.
                            if ( subrchdata(irch,isub,4).le.zerost ) then 
                                Valcon(Ircvld(Irch,WQno))=0.
                            endif  
                            continue
                    */
        }

        protected override double SoilO2mg()
        {
            return 0.0;
        }
    }
}
