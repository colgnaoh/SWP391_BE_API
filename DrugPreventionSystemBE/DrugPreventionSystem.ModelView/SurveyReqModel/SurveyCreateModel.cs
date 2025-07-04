using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel
{
    public class SurveyCreateModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SurveyType SurveyType { get; set; } // enum
    }
}

    
