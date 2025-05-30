using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Helpers
{
    public static class AgeGroupHelper
    {
        public static AgeGroup GetAgeGroup(int age)
        {
            if (age < 18)
                return AgeGroup.Student;
            else if (age <= 26)
                return AgeGroup.UniversityStudent;
            else
                return AgeGroup.Parent;
        }
    }

}
