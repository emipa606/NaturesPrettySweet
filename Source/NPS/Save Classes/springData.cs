using Verse;

namespace TKKN_NPS
{
    public class springData : IExposable
    {
        public int age;
        public string biomeName;
        public int makeAnotherAt;
        public string springID;
        public string status = "spawning";
        public float width;


        public void ExposeData()
        {
            Scribe_Values.Look(ref springID, "springID", "", true);
            Scribe_Values.Look(ref biomeName, "biomeName", "", true);
            Scribe_Values.Look(ref makeAnotherAt, "makeAnotherAt", 0, true);
            Scribe_Values.Look(ref age, "age", 0, true);
            Scribe_Values.Look(ref status, "status", "", true);
            Scribe_Values.Look(ref width, "width", 0, true);
        }
    }
}