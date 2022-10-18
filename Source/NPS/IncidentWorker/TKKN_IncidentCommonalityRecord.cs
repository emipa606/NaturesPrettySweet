using System.Xml;
using RimWorld;
using Verse;

namespace TKKN_NPS;

public class TKKN_IncidentCommonalityRecord
{
    public float commonality;
    public IncidentDef incident;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "incident", xmlRoot.Name);
        commonality = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
    }
}