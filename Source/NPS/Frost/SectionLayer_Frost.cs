using UnityEngine;
using Verse;

namespace TKKN_NPS;

internal class SectionLayer_Frost : SectionLayer
{
    private static readonly Color32 ColorClear = new Color32(194, 219, 249, 0); // 194, 219, 249

    private static readonly Color32 ColorWhite = new Color32(194, 219, 249, 120);
    private readonly float[] vertDepth = new float[9];

    public SectionLayer_Frost(Section section) : base(section)
    {
        relevantChangeTypes = MapMeshFlag.Snow;
    }

    public override bool Visible => true;

    private bool Filled(int index)
    {
        var building = Map.edificeGrid[index];
        return building != null && building.def.Fillage == FillCategory.Full;
    }

    public override void Regenerate()
    {
        var subMesh = GetSubMesh(Verse.MatBases.Snow);
        //		LayerSubMesh subMesh = base.GetSubMesh(MatBases.Frost); // for some reason the custom one was causing a huge memory issue and rendering in giant squares :(

        if (subMesh.mesh.vertexCount == 0)
        {
//				SectionLayerGeometryMaker_Solid.MakeBaseGeometry(this.section, subMesh, AltitudeLayer.MoteLow);
            SectionLayerGeometryMaker_Solid.MakeBaseGeometry(section, subMesh,
                AltitudeLayer.LayingPawn); //so frost forms over items/plants
        }

        subMesh.Clear(MeshParts.Colors);

        var depthGridDirect_Unsafe = Map.GetComponent<FrostGrid>().DepthGridDirect_Unsafe;
        var cellRect = section.CellRect;
        var num = Map.Size.z - 1;
        var num2 = Map.Size.x - 1;
        var b = false;

        var cellIndices = Map.cellIndices;
        for (var i = cellRect.minX; i <= cellRect.maxX; i++) // this is what renders it all blobby, I think
        {
            for (var j = cellRect.minZ; j <= cellRect.maxZ; j++)
            {
                var num3 = depthGridDirect_Unsafe[cellIndices.CellToIndex(i, j)];
                var num4 = cellIndices.CellToIndex(i, j - 1);
                var num5 = j <= 0 ? num3 : depthGridDirect_Unsafe[num4];
                num4 = cellIndices.CellToIndex(i - 1, j - 1);
                var num6 = j <= 0 || i <= 0 ? num3 : depthGridDirect_Unsafe[num4];
                num4 = cellIndices.CellToIndex(i - 1, j);
                var num7 = i <= 0 ? num3 : depthGridDirect_Unsafe[num4];
                num4 = cellIndices.CellToIndex(i - 1, j + 1);
                var num8 = j >= num || i <= 0 ? num3 : depthGridDirect_Unsafe[num4];
                num4 = cellIndices.CellToIndex(i, j + 1);
                var num9 = j >= num ? num3 : depthGridDirect_Unsafe[num4];
                num4 = cellIndices.CellToIndex(i + 1, j + 1);
                var num10 = j >= num || i >= num2 ? num3 : depthGridDirect_Unsafe[num4];
                num4 = cellIndices.CellToIndex(i + 1, j);
                var num11 = i >= num2 ? num3 : depthGridDirect_Unsafe[num4];
                num4 = cellIndices.CellToIndex(i + 1, j - 1);
                var num12 = j <= 0 || i >= num2 ? num3 : depthGridDirect_Unsafe[num4];
                vertDepth[0] = (num5 + num6 + num7 + num3) / 4f;
                vertDepth[1] = (num7 + num3) / 2f;
                vertDepth[2] = (num7 + num8 + num9 + num3) / 4f;
                vertDepth[3] = (num9 + num3) / 2f;
                vertDepth[4] = (num9 + num10 + num11 + num3) / 4f;
                vertDepth[5] = (num11 + num3) / 2f;
                vertDepth[6] = (num11 + num12 + num5 + num3) / 4f;
                vertDepth[7] = (num5 + num3) / 2f;
                vertDepth[8] = num3;
                for (var k = 0; k < 9; k++)
                {
                    if (vertDepth[k] > 0.01f)
                    {
                        b = true;
                    }

                    subMesh.colors.Add(FrostDepthColor(vertDepth[k]));
                }
            }
        }

        if (b)
        {
            subMesh.disabled = false;
            subMesh.FinalizeMesh(MeshParts.Colors);
        }
        else
        {
            subMesh.disabled = true;
        }
    }


    private static Color32 FrostDepthColor(float FrostDepth)
    {
        return Color32.Lerp(ColorClear, ColorWhite, FrostDepth);
    }
}