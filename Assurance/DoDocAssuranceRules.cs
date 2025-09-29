using RiverSystem.Assurance;
using RiverSystem.Management.ExpressionBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using FlowMatters.Source.DODOC.Core;
using RiverSystem;

namespace FlowMatters.Source.DODOC.Assurance
{
    [TimedAssuranceRule(
        RuleCategories.Advanced,
        "DODOC Modelled Area does not exceed Max Area check",
        TimeOfEvaluation.PostFlowPhase,
        LogLevel.Error)]
    public class MaxAccumulationAreaCheck : NetworkRule
    {
        public override ValidationResult Check(Network network, Network item, TimeOfEvaluation timeOfEvaluation,
            DateTime now, double timeStepInSeconds)
        {
            if (FloodplainDoDoc.MaxAreaErrorInfo.ErrorLogged)
            {
                FloodplainDoDoc.MaxAreaErrorInfo.ErrorLogged = false;
                return new ValidationResult(
                    $"DODOC model at {FloodplainDoDoc.MaxAreaErrorInfo.Location} " +
                    $"has a modelled area of {FloodplainDoDoc.MaxAreaErrorInfo.ModelledArea} m2 "+
                    $"that has exceeded the Max Area of {FloodplainDoDoc.MaxAreaErrorInfo.MaxArea} m2.");
            }


            return ValidationResult.Success;
        }
    }
}
