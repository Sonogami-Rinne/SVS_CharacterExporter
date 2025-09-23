using Character;
using CharacterCreation;
using Il2CppSystem.Linq;
using IllusionMods;
using PmxLib;
using SVSExporter;
using SVSExporter.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
internal class PmxBuilder
{

	public string msg = "";

    public HashSet<string> ignoreList = new HashSet<string>
	{
		"Bonelyfans", "c_m_shadowcast", "Standard", "cf_m_body", "cf_m_face_00", "cf_m_tooth", "cf_m_canine", "cf_m_mayuge_00", "cf_m_noseline_00", "cf_m_eyeline_00_up",
		"cf_m_eyeline_kage", "cf_m_eyeline_down", "cf_m_sirome_00", "cf_m_hitomi_00", "cf_m_tang", "cf_m_namida_00", "cf_m_gageye_00", "cf_m_gageye_01", "cf_m_gageye_02",
		"cf_O_face_atari_M", "Highlight_cm_O_face_rend", "cm_m_body", "Highlight_o_body_a_rend", "Highlight_cf_O_face_rend", "o_shadowcaster", "o_body", "cf_O_face", "cf_O_tooth",
		"cf_O_canine", "cf_O_mayuge", "cf_O_noseline", "cf_O_eyeline", "cf_O_eyeline_low", "cf_O_namida_L", "cf_O_namida_M", "cf_O_namida_S", "cf_Ohitomi_L", "cf_Ohitomi_R",
		"cf_Ohitomi_L02", "cf_Ohitomi_R02", "cf_O_gag_eye_00", "cf_O_gag_eye_01", "cf_O_gag_eye_02", "o_tang", "cf_O_face_atari", "o_tango", "o_nail_def01", "o_nail_foot"
    };

	//public static readonly Dictionary<string, string> typeMap = new Dictionary<string, string>
	//{
	//	{ "_MT_CT", "_MainTex_ColorTexture" },
	//	{ "_MT", "_MainTex" },
	//	{ "_AM", "_AlphaMask" },
	//	{ "_CM", "_ColorMask" },
	//	{ "_DM", "_DetailMask" },
	//	{ "_LM", "_LineMask" },
	//	{ "_NM", "_NormalMask" },
	//	{ "_NMP", "_NormalMap" },
	//	{ "_NMPD", "_NormalMapDetail" },
	//	{ "_ot1", "_overtex1" },
	//	{ "_ot2", "_overtex2" },
	//	{ "_ot3", "_overtex3" },
	//	{ "_lqdm", "_liquidmask" },
	//	{ "_HGLS", "_HairGloss" },
	//	{ "_T2", "_Texture2" },
	//	{ "_T3", "_Texture3" },
	//	{ "_T4", "_Texture4" },
	//	{ "_T5", "_Texture5" },
	//	{ "_T6", "_Texture6" },
	//	{ "_T7", "_Texture7" },
	//	{ "_PM1", "_PatternMask1" },
	//	{ "_PM2", "_PatternMask2" },
	//	{ "_PM3", "_PatternMask3" },
	//	{ "_AR", "_AnotherRamp" },
	//	{ "_GLSR", "_GlassRamp" },
	//	{ "_EXPR", "_expression" }
	//};

	public string EyeMatName = "cf_m_hitomi_00";

	public HashSet<string> whitelistOffsetBones = new HashSet<string>
	{
		"cf_d_sk_top", "cf_d_sk_00_00", "cf_j_sk_00_00", "cf_j_sk_00_01", "cf_j_sk_00_02", "cf_j_sk_00_03", "cf_j_sk_00_04", "cf_j_sk_00_05", "cf_d_sk_01_00", "cf_j_sk_01_00",
		"cf_j_sk_01_01", "cf_j_sk_01_02", "cf_j_sk_01_03", "cf_j_sk_01_04", "cf_j_sk_01_05", "cf_d_sk_02_00", "cf_j_sk_02_00", "cf_j_sk_02_01", "cf_j_sk_02_02", "cf_j_sk_02_03",
		"cf_j_sk_02_04", "cf_j_sk_02_05", "cf_d_sk_03_00", "cf_j_sk_03_00", "cf_j_sk_03_01", "cf_j_sk_03_02", "cf_j_sk_03_03", "cf_j_sk_03_04", "cf_j_sk_03_05", "cf_d_sk_04_00",
		"cf_j_sk_04_00", "cf_j_sk_04_01", "cf_j_sk_04_02", "cf_j_sk_04_03", "cf_j_sk_04_04", "cf_j_sk_04_05", "cf_d_sk_05_00", "cf_j_sk_05_00", "cf_j_sk_05_01", "cf_j_sk_05_02",
		"cf_j_sk_05_03", "cf_j_sk_05_04", "cf_j_sk_05_05", "cf_d_sk_06_00", "cf_j_sk_06_00", "cf_j_sk_06_01", "cf_j_sk_06_02", "cf_j_sk_06_03", "cf_j_sk_06_04", "cf_j_sk_06_05",
		"cf_d_sk_07_00", "cf_j_sk_07_00", "cf_j_sk_07_01", "cf_j_sk_07_02", "cf_j_sk_07_03", "cf_j_sk_07_04", "cf_j_sk_07_05"
	};

	public bool exportAllOutfits = false;

    public static int minCoord;

	public static int maxCoord;

	public static int nowCoordinate = -1;

	public static string savePath;

	public static string currentSavePath;

	public static Dictionary<string, int> currentMaterialList = new Dictionary<string, int>();

	public static HashSet<string> currentBonesList = new HashSet<string>();

	public Dictionary<Renderer, List<string>> currentRendererMaterialMapping = new Dictionary<Renderer, List<string>>();

	public static Pmx pmxFile;

	private List<SMRData> characterSMRData = new List<SMRData>();

	private List<MaterialDataComplete> materialDataComplete = new List<MaterialDataComplete>();

	private List<TextureData> textureData = new List<TextureData>();

	//private List<MaterialData> materialData = new List<MaterialData>();

	//private List<ReferenceInfoData> referenceInfoData = new List<ReferenceInfoData>();

	//private List<ChaFileData> chaFileCustomFaceData = new List<ChaFileData>();

	//private List<ChaFileData> chaFileCustomBodyData = new List<ChaFileData>();

	//private List<ChaFileData> chaFileCustomHairData = new List<ChaFileData>();

	private List<CharacterInfoData> characterInfoData = new List<CharacterInfoData>();

	//private List<ChaFileCoordinateData> chaFileCoordinateData = new List<ChaFileCoordinateData>();

	private List<DynamicBoneData> dynamicBonesData = new List<DynamicBoneData>();

	private List<DynamicBoneColliderData> dynamicBoneCollidersData = new List<DynamicBoneColliderData>();

	//private List<AccessoryStateData> accessoryStateData = new List<AccessoryStateData>();

	//private List<ListInfoData> listInfoData = new List<ListInfoData>();

	private List<BoneOffsetData> boneOffsetData = new List<BoneOffsetData>();

	private static Dictionary<int, int> instanceIDs = new Dictionary<int, int>();

	private static HashSet<string> offsetBoneCandidates = new HashSet<string>();

	private int vertexCount;

	private readonly int scale = 1;

	private Dictionary<string, int> currentBoneKeysList = new Dictionary<string, int>();

	private Dictionary<string, int> vertexCountRecord = new Dictionary<string, int>();

    private List<BoneInfo> editBoneInfo = new List<BoneInfo>();

	private List<object> recoverInfos = new List<object>();

    private Dictionary<string,BoneInfo> finalBoneInfo = new Dictionary<string, BoneInfo>();

	private List<BoneInfo> finalBoneInfoCache = new List<BoneInfo>();

    private List<Renderer> meshRenders = new List<Renderer>();

	private List<UVAdjustment> uvAdjustments = new List<UVAdjustment>();

	private Dictionary<string, List<string>> smrMaterialsCache = new Dictionary<string, List<string>>();

	private GameObject gameObjectMeshCopier;

    private string charaName;


	public static void test()
	{
        Human human = SVSExporterPlugin.selectedChara;
        Dictionary<string, Mesh> meshes = new();

        var fs = new Il2CppSystem.IO.FileStream("abdata/chara/head/head_02.unity3d", Il2CppSystem.IO.FileMode.Open, Il2CppSystem.IO.FileAccess.Read);
        var bundle = AssetBundle.LoadFromStream(fs);

        foreach (var i in bundle.LoadAllAssets())
        {
            if (i.name == "p_cf_sv_head_01_low")
            {
                GameObject ga = i.TryCast<GameObject>();
                var instance = UnityEngine.Object.Instantiate(ga);
                instance.name = "Morph info carrier";
                foreach (var j in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    meshes.Add(j.sharedMesh.name, j.sharedMesh);
                }
                break;
            }
        }
        bundle.Unload(false);
        fs.Close();

        if (meshes.Count == 0)
        {
            Console.WriteLine("Load from assetbundle failed.Bundle: " + "abdata/chara/head/head_02.unity3d");
            return;
        }

        var fBSTarget = human.face.eyesCtrl.FBSTarget;


        for (int i = 0; i < fBSTarget.Length; i++)
        {
            var smr = fBSTarget[i].GetSkinnedMeshRenderer();
			Mesh mesh = smr.sharedMesh;
   //         Mesh mesh = meshes[CleanUpName(smr.sharedMesh.name)];
			//Console.WriteLine(mesh.isReadable + " " + mesh.blendShapeCount);

			Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> array = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.vertices.Length);
			Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> deltaNormals = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.normals.Length);
			Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> deltaTangents = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.tangents.Length);
            //UnityEngine.Vector3[] array = new UnityEngine.Vector3[mesh.vertices.Length];
            //UnityEngine.Vector3[] deltaNormals = new UnityEngine.Vector3[mesh.normals.Length];
            //UnityEngine.Vector3[] deltaTangents = new UnityEngine.Vector3[mesh.tangents.Length];

            for (int j = 0; j < mesh.blendShapeCount; j++)
            {
				Console.WriteLine(mesh.GetBlendShapeFrameCount(j));
                mesh.GetBlendShapeFrameVertices(j, 0, array, deltaNormals, deltaTangents);
                for (int k = 0; k < array.Length; k++)
                {
					if (array[k].x != 0 || array[k].y != 0 || array[k].z != 0)
					{
						Console.WriteLine(array[k].x + " " + array[k].y + " " + array[k].z);
					}

                }
            }
        }
    }

	public IEnumerator BuildStart()
	{
        Human human = SVSExporterPlugin.selectedChara;
		//human.cloth.SetClothesState()
        this.charaName = human.fileParam.GetCharaName(false);
        CreateBaseSavePath();
        ChangeAnimations();
        CollectIgnoreList();
        CreateCharacterInfoData();
		Prepare();
        nowCoordinate = exportAllOutfits ? 0 : human.fileStatus.coordinateType;
		maxCoord = exportAllOutfits ? human.coorde.data.Coordinates.Length : nowCoordinate + 1;

		for (; nowCoordinate < maxCoord + 1; nowCoordinate++)
		{
			if (nowCoordinate < maxCoord)
			{
				human.coorde.ChangeCoordinateType((ChaFileDefine.CoordinateType)nowCoordinate);
				human.ReloadCoordinate(Human.ReloadFlags.Coorde);
                human.body.ResetDynamicBoneAll();
                yield return new WaitForSeconds(0.2f);
			}
            BuildStart_BG();
            yield return new WaitForSeconds(0.2f);
        }
		ExportAllDataLists();
		CleanUp();
		OpenFolderInExplorer(savePath);
		Console.WriteLine("Finished");
    }

    private void BuildStart_BG()
	{
		try
		{
			ResetPmxBuilder();
			CreateModelInfo();
			CreateInstanceIDs();
            SetSavePath();
            Directory.CreateDirectory(currentSavePath);
            Directory.CreateDirectory(currentSavePath + "/pre_light");
            Directory.CreateDirectory(currentSavePath + "/pre_dark");
            ClearMorphs();

            //SetSkinnedMeshList();
			PrepareModel();

            if (nowCoordinate < maxCoord)
            {
                CreateBoneList();
            }
            CreateMeshList();

            if (nowCoordinate == maxCoord)
            {
            	CreateMorph();
				//ExportGagEyes();
			}
			AddAccessory();
			ExportLightTexture();
			if (nowCoordinate < maxCoord)
			{
                try
                {
                    CreateDynamicBonesData();
                    CreateDynamicBoneCollidersData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

			CreatePmxHeader();
			Save();

		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
		}
   
	}
	
	private void Prepare()
	{
        gameObjectMeshCopier = new GameObject("MeshCopier");
        gameObjectMeshCopier.AddComponent<SkinnedMeshRenderer>();

        GameObject.Find("Cvs_BackGround").GetComponent<Canvas>().enabled = false;
        Camera camera = gameObjectMeshCopier.AddComponent<Camera>();
        GameObject light = Light.FindObjectsOfType<Light>()[0].gameObject;

        camera.CopyFrom(Camera.main);
		recoverInfos.Clear();
        recoverInfos.Add(light.transform.rotation);
        recoverInfos.Add(light.transform.position);
        recoverInfos.Add(camera.transform.position);
        recoverInfos.Add(camera.transform.rotation);

        camera.orthographic = true;
        camera.aspect = 1f;

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.allowHDR = false;
        camera.allowMSAA = false;
        camera.cullingMask = 1 << 10;

		//human.body.LoadAnimation("custom/00.unity3d", "tpose");
		
    }
	private void SetSavePath()
	{
        if (nowCoordinate == maxCoord)
        {
            currentSavePath = savePath;
        }
        else
        {
            currentSavePath = savePath + "Outfit " + nowCoordinate.ToString("00") + "/";
        }
    } 
	public void PrepareModel()
	{
        Transform transform = GameObject.Find("BodyTop").transform;
        Transform[] componentsInChildren = transform.GetComponentsInChildren<Transform>(includeInactive: true);

		finalBoneInfoCache.Clear();
        foreach (Transform component in componentsInChildren)
        {
            finalBoneInfoCache.Add(new BoneInfo(component.name, component));
        }
        foreach (Transform component in componentsInChildren)
        {
			component.localScale = UnityEngine.Vector3.one;
        }
		meshRenders.Clear();
		smrMaterialsCache.Clear();
    }

    public void CleanUp()
	{
		try
		{
			GameObject.Destroy(gameObjectMeshCopier);

			//foreach (var info in finalBoneInfo.Values)
			//{
			//	info.targetTransform.localScale = info._scale;
			//}
            finalBoneInfo.Clear();
            editBoneInfo.Clear();
			uvAdjustments.Clear();

            GameObject light = Light.FindObjectsOfType<Light>()[0].gameObject;
            Camera camera = Camera.main;

            light.transform.rotation = (UnityEngine.Quaternion)recoverInfos[0];
            light.transform.position = (UnityEngine.Vector3)recoverInfos[1];

            camera.transform.position = (UnityEngine.Vector3)recoverInfos[2];
            camera.transform.rotation = (UnityEngine.Quaternion)recoverInfos[3];

            GameObject.Find("Cvs_BackGround").GetComponent<Canvas>().enabled = true;

            recoverInfos.Clear();
        }
		catch(Exception ex)
		{
			Console.WriteLine("Error when cleaning up, reason:" + ex.Message);
		}
		finally
		{
			Human human = SVSExporterPlugin.selectedChara;
			human.Reload();
		}
	}
    public void ExportLightTexture()
    {
        GameObject.Find("BodyTop").transform.Translate(new UnityEngine.Vector3(0, 10, 0));

        string[] ignoredSMRs = { "cf_O_gag_eye_00", "cf_O_gag_eye_01", "cf_O_gag_eye_02", "cf_O_namida_L", "cf_O_namida_M", "cf_O_namida_S", "Highlight_o_body_a_rend", "Highlight_cf_O_face_rend", "o_Mask" };
		string[] ignoredShaders = { "LIF/lit_hair_overlay", "LIF/lif_main_hair_outline" };
        GameObject light = Light.FindObjectsOfType<Light>()[0].gameObject;// The scene has only one light.But I'm failed to get the light by its name
        Camera camera = gameObjectMeshCopier.GetComponent<Camera>();
        Camera cameraMain = Camera.main;
        SkinnedMeshRenderer meshCopier = gameObjectMeshCopier.GetComponent<SkinnedMeshRenderer>();
        UnityEngine.Quaternion lightRotation = new UnityEngine.Quaternion(-0.03940f, 0.95533f, -0.21508f, -0.19882f);
        UnityEngine.Quaternion darkRotation = new UnityEngine.Quaternion(0.16412f, -0.02926f, 0.00487f, 0.98600f);
        UnityEngine.Vector3 lightPosition = new UnityEngine.Vector3(0.70f, 4.46f, 2.69f);
        UnityEngine.Vector3 darkPosition = new UnityEngine.Vector3(-0.17f, 4.40f, 2.84f);

        for (int i = 0; i < meshRenders.Count; i++)
        {
            var smr = meshRenders[i];
            string smrName = smr.name;

            bool flag = false;
            smrName = ((ignoreList.Contains(smrName, StringComparer.Ordinal) && smr.sharedMaterials.Length > 0 && ignoreList.Contains(PmxBuilder.CleanUpName(smr.sharedMaterial.name), StringComparer.Ordinal)) ? smrName : (smrName + " " + PmxBuilder.GetAltInstanceID(smr)));
            for (int j = 0; j < ignoredSMRs.Length; j++)
            {
                if (smrName.Contains(ignoredSMRs[j]))
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                continue;
            }
            Console.WriteLine("Exporting textures for " + smr.name);
            Mesh mesh = new Mesh();
            LightProbeUsage probeUsage;
            Transform probeAnchor;
            ShadowCastingMode probeCastingMode;
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            bool receiveShadow;
            int subMeshCount = mesh.subMeshCount;
            if (smr.GetType().Name == "SkinnedMeshRenderer")
            {
                var _tmp = (SkinnedMeshRenderer)smr;
                _tmp.BakeMesh(mesh);
                //mesh.colors = _tmp.sharedMesh.colors;
                probeUsage = _tmp.lightProbeUsage;
                probeAnchor = _tmp.probeAnchor;
                probeCastingMode = _tmp.shadowCastingMode;
                receiveShadow = _tmp.receiveShadows;
                _tmp.GetPropertyBlock(materialPropertyBlock);

            }
            else
            {
                MeshFilter meshFilter = smr.gameObject.GetComponent<MeshFilter>();
                var _tmp = (MeshRenderer)smr;
                if (meshFilter.mesh != null)
                {
                    meshCopier.sharedMesh = meshFilter.mesh;
                }
                else
                {
                    meshCopier.sharedMesh = meshFilter.sharedMesh;
                }
                meshCopier.BakeMesh(mesh);
                meshCopier.sharedMesh = null;

                probeUsage = _tmp.lightProbeUsage;
                probeAnchor = _tmp.probeAnchor;
                probeCastingMode = _tmp.shadowCastingMode;
                receiveShadow = _tmp.receiveShadows;
                _tmp.GetPropertyBlock(materialPropertyBlock);

            }
            if (probeAnchor != null)
            {
                Console.WriteLine(probeAnchor.position + " " + probeAnchor.forward);
            }

            Il2CppSystem.Collections.Generic.List<UnityEngine.Vector2> uvs = new();
			for(int j = 0;j < 8; j++)
			{
				mesh.GetUVs(j, uvs);
				if (uvs != null && uvs.Count > 0)
				{
					break;
				}

			}
			if (uvs == null || uvs.Count == 0)
			{
                Console.WriteLine("No uv map, ignore");
				continue;
            }

            int horizontalBlockCount = 1;
            int verticalBlockCount = 1;
            int xOffset;
            int yOffset;
            int layer = smr.gameObject.layer;// Do not forget to drawmesh in this layer
            UnityEngine.Vector3[] verts = new UnityEngine.Vector3[uvs.Count];
            // Transform mesh into a surface according to its uv
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            for (int j = 0; j < uvs.Count; j++)
            {
                verts[j] = new UnityEngine.Vector3(-uvs[j].x, uvs[j].y, 0f);
                minX = Mathf.Min(minX, uvs[j].x);
                maxX = Mathf.Max(maxX, uvs[j].x);
                minY = Mathf.Min(minY, uvs[j].y);
                maxY = Mathf.Max(maxY, uvs[j].y);
            }

            xOffset = ((int)Math.Floor(minX));
            yOffset = ((int)Math.Floor(minY));
            horizontalBlockCount = (int)Math.Ceiling(maxX) - xOffset;
            verticalBlockCount = (int)Math.Ceiling(maxY) - yOffset;

            // Correct triangle if its normal direction is not positive z
            var triangles = mesh.triangles;
            for (int j = 0; j < triangles.Length; j += 3)
            {
                var v0 = verts[triangles[j]];
                var v1 = verts[triangles[j + 1]];
                var v2 = verts[triangles[j + 2]];

                if (UnityEngine.Vector3.Cross(v1 - v0, v2 - v0).z < 0)
                {
                    var tmp = triangles[j + 1];
                    triangles[j + 1] = triangles[j + 2];
                    triangles[j + 2] = tmp;
                }
            }

            uvIslandSolver(triangles, verts);

            mesh.vertices = verts;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            List<string> materials = new List<string>();
            uvAdjustments.Add(new UVAdjustment(smrName, PmxBuilder.GetGameObjectPath(meshRenders[i].gameObject), -xOffset, -yOffset, horizontalBlockCount, verticalBlockCount, materials));

            var positionFront = new UnityEngine.Vector3(-xOffset - horizontalBlockCount / 2.0f, yOffset + verticalBlockCount / 2.0f, 10f);
            var positionLookAt = new UnityEngine.Vector3(positionFront.x, positionFront.y, 0f);

            camera.orthographicSize = verticalBlockCount / 2.0f;
            camera.aspect = (float)horizontalBlockCount / verticalBlockCount;
            camera.transform.position = positionFront;
            camera.transform.LookAt(positionLookAt);
            cameraMain.transform.position = positionFront;
            cameraMain.transform.LookAt(positionLookAt);

            for (int j = 0; j < Math.Min(smr.sharedMaterials.Length, subMeshCount); j++)
            {
                if (meshRenders[i].sharedMaterials[j] == null || ignoredShaders.Contains(meshRenders[i].sharedMaterials[j].shader.name)) continue;
                Material material = new Material(meshRenders[i].sharedMaterials[j]);

                string matName = smrMaterialsCache[GetGameObjectPath(smr.gameObject)][j];

                materials.Add(matName);
                int texturewidth;
                int textureheight;
                try
                {
                    Texture mainTex = null;
                    if (material.HasProperty("_Main_texture"))
                    {
                        mainTex = material.GetTexture("_Main_texture");
                    }
                    else if (material.HasProperty("_Create_main_texture"))
                    {
                        mainTex = material.GetTexture("_Create_main_texture");
                    }
                    if (mainTex != null)
                    {
                        int baseLength = Math.Max(mainTex.width, mainTex.height);
						if (baseLength <= 0)
						{
							baseLength = 1024;
						}
                        texturewidth = baseLength * horizontalBlockCount;
                        textureheight = baseLength * verticalBlockCount;
                    }
                    else
                    {
                        texturewidth = 1024 * horizontalBlockCount;
                        textureheight = 1024 * verticalBlockCount;
                    }

                    if (material.HasProperty("_DetailNormal"))
                    {
                        material.SetTexture("_DetailNormal", null);
                    }
                    if (material.HasProperty("_Matcap_mask"))
                    {
                        material.SetTexture("_Matcap_mask", null);
                    }
                    if (material.HasProperty("_Mat_cap_01"))
                    {
                        material.SetTexture("_Mat_cap_01", null);
                    }
                    if (material.HasProperty("_Mat_cap_02"))
                    {
                        material.SetTexture("_Mat_cap_02", null);
                    }
                    if (material.HasProperty("_Mat_cap_03"))
                    {
                        material.SetTexture("_Mat_cap_03", null);
                    }
                    if (material.HasProperty("unity_Lightmaps"))
                    {
                        material.SetTexture("unity_Lightmaps", null);
                    }
                    if (material.HasProperty("unity_LightmapsInd"))
                    {
                        material.SetTexture("unity_LightmapsInd", null);
                    }
                    if (material.HasProperty("_Normal"))
                    {
                        material.SetTexture("_Normal", null);
                    }
                    if (material.HasProperty("_Normal_map"))
                    {
                        material.SetTexture("_Normal_map", null);
                    }
                    if (material.HasProperty("_Cloth_alpha"))
                    {
                        material.SetTexture("_Cloth_alpha", null);
                    }
                    if (material.HasProperty("_Cloth_alpha_bot"))
                    {
                        material.SetTexture("_Cloth_alpha_bot", null);
                    }

                    Color32[] lightColor;
                    Color32[] darkColor;
                    Thread lightThread;
                    Thread darkThread;

                    lightColor = render(lightRotation, lightPosition, j);
                    lightThread = new Thread(() => shiftAndOverlay(lightColor, 2));
                    lightThread.Start();

                    darkColor = render(darkRotation, darkPosition, j);
                    darkThread = new Thread(() => shiftAndOverlay(darkColor, 2));
                    darkThread.Start();


                    lightThread.Join();
                    darkThread.Join();

                    TextureSaver.SaveTexture(lightColor, texturewidth, textureheight, currentSavePath + "/pre_light/" + matName + "_light.png");
                    TextureSaver.SaveTexture(darkColor, texturewidth, textureheight, currentSavePath + "/pre_dark/" + matName + "_dark.png");

                    Color32[] render(UnityEngine.Quaternion rotation, UnityEngine.Vector3 position, int subMeshIndex)
                    {
                        light.transform.rotation = rotation;
                        light.transform.position = position;
                        GL.Flush();

                        RenderTexture renderTexture = new RenderTexture(texturewidth, textureheight, 24, RenderTextureFormat.ARGB32);
                        renderTexture.antiAliasing = 1;
                        renderTexture.filterMode = FilterMode.Point;
                        renderTexture.Create();

                        camera.targetTexture = renderTexture;
                        camera.backgroundColor = Color.clear;
                        Graphics.DrawMesh(mesh, Matrix4x4.identity, material, layer, camera, 0, materialPropertyBlock, probeCastingMode, receiveShadow, probeAnchor, probeUsage);
                        camera.Render();

                        RenderTexture.active = renderTexture;
                        Texture2D _ = new Texture2D(texturewidth, textureheight, TextureFormat.ARGB32, false);
                        _.ReadPixels(new Rect(0, 0, texturewidth, textureheight), 0, 0);
                        _.Apply();
                        RenderTexture.active = null;
                        var data = _.GetPixels32();
                        Texture.Destroy(_);
                        camera.targetTexture = null;
                        renderTexture.Release();
                        return data;
                    }

                    void shiftAndOverlay(Color32[] color, int offset)
                    {
                        //To fix that color around the edge of UV island will be mixed with background color(transparent)
                        var basePixels = new Color32[color.Length];
                        Array.Copy(color, basePixels, color.Length);
                        // Add 4 directions' shift to original texture and mix them.Then add the original texture on it.
                        void AddShift(int dx, int dy)
                        {
                            for (int y = 0; y < textureheight; y++)
                            {
                                for (int x = 0; x < texturewidth; x++)
                                {
                                    int sx = x - dx;
                                    int sy = y - dy;
                                    if (sx >= 0 && sx < texturewidth && sy >= 0 && sy < textureheight)
                                    {
                                        int dstIdx = y * texturewidth + x;
                                        int srcIdx = sy * texturewidth + sx;
                                        var srcColor = basePixels[srcIdx];

                                        if (srcColor.a > color[dstIdx].a)
                                        {
                                            color[dstIdx] = srcColor;
                                        }
                                    }
                                }
                            }
                        }
                        AddShift(offset, 0);
                        AddShift(-offset, 0);
                        AddShift(0, offset);
                        AddShift(0, -offset);

                        for (int i = 0; i < color.Length; i++)
                        {
                            // Put the original material to the top.If the extended color area covers other UV islands, use this to fix. 
                            if (basePixels[i].a > 250)
                            {
                                color[i] = basePixels[i];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (material != null)
                    {
                        Material.Destroy(material);
                    }
                }
            }
            if (mesh != null)
            {
                Mesh.Destroy(mesh);
            }
        }
        
		GameObject.Find("BodyTop").transform.Translate(new UnityEngine.Vector3(0, -10, 0));
        
		void uvIslandSolver(int[] triangles, UnityEngine.Vector3[] vertices)
        {
            int[] parent = new int[vertices.Length];
            float[] minX = new float[vertices.Length];
            float[] minY = new float[vertices.Length];
            float[] maxX = new float[vertices.Length];
            float[] maxY = new float[vertices.Length];

            int findAndUpdate(int index)
            {
                Stack<int> stack = new Stack<int>();
                while (parent[index] != index)
                {
                    stack.Push(index);
                    index = parent[index];
                }
                while (stack.Count > 0)
                {
                    int cur = stack.Pop();
                    parent[cur] = index;
                }
                return index;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                parent[i] = i;
                var vertex = vertices[i];
                minX[i] = vertex.x;
                maxX[i] = vertex.x;
                minY[i] = vertex.y;
                maxY[i] = vertex.y;
            }

            for (int i = 0; i < triangles.Length; i += 3)
            {
                var parentIndex = findAndUpdate(parent[triangles[i]]);
                var _parentIndex1 = findAndUpdate(parent[triangles[i + 1]]);
                var _parentIndex2 = findAndUpdate(parent[triangles[i + 2]]);

                parent[_parentIndex1] = parentIndex;
                parent[_parentIndex2] = parentIndex;

                minX[parentIndex] = Math.Min(minX[parentIndex], minX[_parentIndex1]);
                minX[parentIndex] = Math.Min(minX[parentIndex], minX[_parentIndex2]);

                minY[parentIndex] = Math.Min(minY[parentIndex], minY[_parentIndex1]);
                minY[parentIndex] = Math.Min(minY[parentIndex], minY[_parentIndex2]);

                maxX[parentIndex] = Math.Max(maxX[parentIndex], maxX[_parentIndex1]);
                maxX[parentIndex] = Math.Max(maxX[parentIndex], maxX[_parentIndex2]);

                maxY[parentIndex] = Math.Max(maxY[parentIndex], maxY[_parentIndex1]);
                maxY[parentIndex] = Math.Max(maxY[parentIndex], maxY[_parentIndex2]);
            }
            List<List<int>> islands = new List<List<int>>();
            Dictionary<int, float> offsets = new Dictionary<int, float>();

            for (int i = 0; i < vertices.Length; i++)
            {
                int parentIndex = findAndUpdate(parent[i]);
                if (offsets.TryGetValue(parentIndex, out float offset))
                {
                    var _v = vertices[i];
                    vertices[i] = new UnityEngine.Vector3(_v.x, _v.y, offset);
                    continue;
                }
                offset = 0f;
                bool flag = false;

                for (int layer = 0; layer < islands.Count; layer++)
                {
                    flag = true;
                    var layerInfo = islands[layer];
                    for (int j = 0; j < islands[layer].Count; j++)
                    {
                        int _parentIndex = layerInfo[j];
                        if (!(minX[_parentIndex] >= maxX[parentIndex] || maxX[_parentIndex] <= minX[parentIndex] || minY[_parentIndex] >= maxY[parentIndex] || maxY[_parentIndex] <= minY[parentIndex]))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        offsets.Add(parentIndex, offset);
                        layerInfo.Add(parentIndex);
                        var _v = vertices[i];
                        vertices[i] = new UnityEngine.Vector3(_v.x, _v.y, offset);
                        break;
                    }
                    offset -= 0.0001f;
                }
                if (!flag)
                {
                    offsets.Add(parentIndex, offset);
                    islands.Add(new List<int>() { parentIndex });
                }
            }
        }
    }
    public void ChangeAnimations()
	{
		Human human = SVSExporterPlugin.selectedChara;
		HumanFace face = human.face;
		face.human.face.ChangeEyesBlinkFlag(false);
		face.ChangeEyesShaking(false);
		face.ChangeLookEyesTarget(1, null, 0f);
		face.ChangeEyesPtn(1, false);
		face.ChangeEyesPtn(0, false);
		face.ChangeMouthPtn(1, false);
		face.ChangeMouthPtn(0, false);
		face.ChangeEyebrowPtn(1, false);
		face.ChangeEyebrowPtn(0, false);
		face.ChangeEyesOpenMax(1);
		face.ChangeMouthOpenMax(0);
		
		human.body.animBody.speed = 0f;

		// Need a T-Pose mod
		human.body.animBody.Play(human.body.animBody.runtimeAnimatorController.animationClips[0].name);
	}

    public void CreateBaseSavePath()
    {
        //Use the card's name in the save path, but remove any invalid characters to prevent save issues
        //string characterName = Singleton<CustomBase>.Instance.chaCtrl.chaFile.parameter.fullname.Replace(" ", string.Empty);
        //characterName = string.Join("_", characterName.Split(Path.GetInvalidFileNameChars()));
        string exportFolder = SVSExporterPlugin.basePath;
        savePath = Path.Combine(Application.dataPath, $"../{exportFolder}/{DateTime.Now:yyyyMMddHHmmss}_{charaName}/");
        Directory.CreateDirectory(savePath);
    }

    private void ResetPmxBuilder()
	{
		currentMaterialList.Clear();
		currentRendererMaterialMapping.Clear();
		List<PmxBone> boneList = new List<PmxBone>();
		Dictionary<string, Pmx.BackupBoneData> boneBackupData = new Dictionary<string, Pmx.BackupBoneData>();
		if (pmxFile != null)
		{
			boneList = pmxFile.BoneList;
			boneBackupData = pmxFile.BoneBackupData;
		}
		pmxFile = new Pmx
		{
			BoneList = boneList,
			BoneBackupData = boneBackupData
		};
		vertexCount = 0;
		vertexCountRecord.Clear();

    }

	public void CreateModelInfo()
	{
		PmxModelInfo pmxModelInfo = new PmxModelInfo
		{
			ModelName = "Summer Vacation Scramble",
			ModelNameE = "",
			Comment = "Exported Summer Vacation Scramble"
		};
		pmxModelInfo.Comment = "";
		pmxFile.ModelInfo = pmxModelInfo;
	}

	public void CreateInstanceIDs()
	{
		instanceIDs.Clear();
		Transform[] componentsInChildren = GameObject.Find("BodyTop").transform.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!(componentsInChildren[i].gameObject == null))
			{
				instanceIDs.Add(componentsInChildren[i].gameObject.GetInstanceID(), instanceIDs.Count);
			}
		}
		Component[] componentsInChildren2 = GameObject.Find("BodyTop").transform.GetComponentsInChildren<Component>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (!(componentsInChildren2[j] == null))
			{
				instanceIDs.Add(componentsInChildren2[j].GetInstanceID(), instanceIDs.Count);
			}
		}
	}

	public void CollectIgnoreList()
	{
		Human human = SVSExporterPlugin.selectedChara;
		
		Renderer[] componentsInChildren = human.face.objHead.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!ignoreList.Contains(renderer.name, StringComparer.Ordinal))
			{
				ignoreList.Add(renderer.name);
			}
			//Material[] materials = renderer.materials;
			//for (int j = 0; j < materials.Length; j++)
			//{
			//	string text = CleanUpName(materials[j].name);
			//	if (!ignoreList.Contains(text, StringComparer.Ordinal))
			//	{
			//		ignoreList.Add(text);
			//	}
			//}
		}
		var rendEye = human.face.rendEye;
		if (rendEye[0] != null && rendEye[0].material != null)
		{
			EyeMatName = CleanUpName(rendEye[0].material.name);
		}
	}

	public void CreateMeshList()
	{
		//string[] source = new string[0];
		//string[] source2 = new string[] { "cf_O_namida_L", "cf_O_namida_M", "cf_O_namida_S", "cf_O_gag_eye_00", "o_tang" };
		string[] source3 = new string[8] { "o_mnpa", "o_mnpb", "n_tang", "n_tang_silhouette", "o_dankon", "o_gomu", "o_dan_f", "cf_O_canine" };
		//string[] source4 = new string[23]
		//{
		//	"o_hit_armL", "o_hit_armR", "o_hit_footL", "o_hit_footR", "o_hit_handL", "o_hit_handR", "o_hit_hara", "o_hit_haraB", "o_hit_haraUnder", "o_hit_kneeBL",
		//	"o_hit_kneeBR", "o_hit_kneeL", "o_hit_kneeR", "o_hit_kokan", "o_hit_legBL", "o_hit_legBR", "o_hit_legL", "o_hit_legR", "o_hit_mune", "o_hit_muneB",
		//	"o_hit_siriL", "o_hit_siriR", "cf_O_face_atari"
		//};
		SkinnedMeshRenderer[] componentsInChildren = GameObject.Find("BodyTop").transform.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive:true);
		int total = 0;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (
                (
                    nowCoordinate < maxCoord
                    &&
                    (
						(!componentsInChildren[i].enabled || !componentsInChildren[i].isVisible)
						||
						ignoreList.Contains(componentsInChildren[i].name, StringComparer.Ordinal)
						||
						componentsInChildren[i].sharedMaterials.Length == 0
					)
                )
                || 
				(
					nowCoordinate == maxCoord 
					&&
                    !ignoreList.Contains(componentsInChildren[i].name, StringComparer.Ordinal)
                )
				||
                source3.Contains(componentsInChildren[i].name, StringComparer.Ordinal)

            )
			{
				continue;
			}
			
			meshRenders.Add(componentsInChildren[i]);
            Console.WriteLine("Exporting: " + componentsInChildren[i].name);
			SMRData sMRData = new SMRData(this, componentsInChildren[i]);
			AddToSMRDataList(sMRData);
			smrMaterialsCache.Add(sMRData.SMRPath, sMRData.SMRMaterialNames);
			MaterialDataComplete matData = new MaterialDataComplete(this, componentsInChildren[i]);
			AddToMaterialDataCompleteList(matData);
			if (currentRendererMaterialMapping.ContainsKey(componentsInChildren[i]))
			{
				Console.WriteLine("Issue - Renderer already added to Material name cache: " + sMRData.SMRName);
			}
			else
			{
				currentRendererMaterialMapping.Add(componentsInChildren[i], sMRData.SMRMaterialNames);
			}

			Mesh mesh = new Mesh();
            componentsInChildren[i].BakeMesh(mesh);

			if (nowCoordinate == maxCoord)
			{
				vertexCountRecord.Add(componentsInChildren[i].name + componentsInChildren[i].sharedMaterial.name, total);
				total += mesh.vertexCount;
            }
			//_ = componentsInChildren[i].gameObject;
			BoneWeight[] boneWeights = componentsInChildren[i].sharedMesh.boneWeights;
			Transform transform = componentsInChildren[i].gameObject.transform;
			int bone = sbi(GetAltBoneName(transform), transform.GetInstanceID().ToString());
			UnityEngine.Vector2[] uv = mesh.uv;
			List<UnityEngine.Vector2[]> list = new List<UnityEngine.Vector2[]> { mesh.uv2, mesh.uv3, mesh.uv4 };
			//_ = mesh.colors;
			UnityEngine.Vector3[] normals = mesh.normals;
			UnityEngine.Vector3[] vertices = mesh.vertices;
			for (int j = 0; j < mesh.subMeshCount; j++)
			{
				int[] triangles = mesh.GetTriangles(j);
				AddFaceList(triangles, vertexCount);
				if (j < componentsInChildren[i].sharedMaterials.Length)
				{
					CreateMaterial(componentsInChildren[i].sharedMaterials[j], sMRData.SMRMaterialNames[j], triangles.Length);
				}
				else if (componentsInChildren[i].sharedMaterial != null)
				{
					CreateMaterial(componentsInChildren[i].sharedMaterial, sMRData.SMRMaterialNames[0], triangles.Length);
				}
			}
			if (string.CompareOrdinal(componentsInChildren[i].name, "cf_O_eyeline") == 0)
			{
				int[] triangles2 = mesh.GetTriangles(0);
				AddFaceList(triangles2, vertexCount);
				for (int k = 1; k < 2; k++)
				{
					CreateMaterial(componentsInChildren[i].sharedMaterials[k], sMRData.SMRMaterialNames[k], triangles2.Length);
				}
			}
			vertexCount += mesh.vertexCount;
			for (int l = 0; l < mesh.vertexCount; l++)
			{
				PmxVertex pmxVertex = new PmxVertex
				{
					UV = new PmxLib.Vector2(uv[l].x, (float)((double)(0f - uv[l].y) + 1.0))
				};
				for (int m = 0; m < list.Count; m++)
				{
					if (list[m].Length != 0)
					{
						pmxVertex.UVA[m] = new PmxLib.Vector4(list[m][l].x, (float)((double)(0f - list[m][l].y) + 1.0), 0f, 0f);
					}
				}
				if (boneWeights.Length != 0)
				{
					pmxVertex.Weight = ConvertBoneWeight(boneWeights[l], componentsInChildren[i].bones);
				}
				else
				{
					pmxVertex.Weight = new PmxVertex.BoneWeight[4];
					pmxVertex.Weight[0].Bone = bone;
					pmxVertex.Weight[0].Value = 1f;
				}
				UnityEngine.Vector3 vector = componentsInChildren[i].transform.TransformDirection(normals[l]);
				pmxVertex.Normal = new PmxLib.Vector3(0f - vector.x, vector.y, 0f - vector.z);
                //UnityEngine.Vector3 vector2 = componentsInChildren[i].transform.TransformPointUnscaled(vertices[l]);
                UnityEngine.Vector3 vector2 = componentsInChildren[i].transform.TransformPoint(vertices[l]);
                pmxVertex.Position = new PmxLib.Vector3((0f - vector2.x) * (float)scale, vector2.y * (float)scale, (0f - vector2.z) * (float)scale);
				pmxVertex.Deform = PmxVertex.DeformType.BDEF4;
				pmxFile.VertexList.Add(pmxVertex);
			}
            Mesh.Destroy(mesh);
        }
	}

    private PmxVertex.BoneWeight[] ConvertBoneWeight(BoneWeight unityWeight, Transform[] bones)
	{
		PmxVertex.BoneWeight[] array = new PmxVertex.BoneWeight[4];
		try
		{
			if (unityWeight.boneIndex0 >= 0 && unityWeight.boneIndex0 < bones.Length)
			{
				array[0].Bone = sbi(GetAltBoneName(bones[unityWeight.boneIndex0]), bones[unityWeight.boneIndex0].GetInstanceID().ToString());
			}
			array[0].Value = unityWeight.weight0;
			if (unityWeight.boneIndex1 >= 0 && unityWeight.boneIndex0 < bones.Length)
			{
				array[1].Bone = sbi(GetAltBoneName(bones[unityWeight.boneIndex1]), bones[unityWeight.boneIndex1].GetInstanceID().ToString());
			}
			array[1].Value = unityWeight.weight1;
			if (unityWeight.boneIndex2 >= 0 && unityWeight.boneIndex0 < bones.Length)
			{
				array[2].Bone = sbi(GetAltBoneName(bones[unityWeight.boneIndex2]), bones[unityWeight.boneIndex2].GetInstanceID().ToString());
			}
			array[2].Value = unityWeight.weight2;
			if (unityWeight.boneIndex3 >= 0 && unityWeight.boneIndex0 < bones.Length)
			{
				array[3].Bone = sbi(GetAltBoneName(bones[unityWeight.boneIndex3]), bones[unityWeight.boneIndex3].GetInstanceID().ToString());
			}
			array[3].Value = unityWeight.weight3;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return array;
	}

	private void ClearMorphs()
	{
        Human human = SVSExporterPlugin.selectedChara;

		var fBSTarget = human.face.eyesCtrl.FBSTarget;
		for (int i = 0; i < fBSTarget.Length; i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = fBSTarget[i].GetSkinnedMeshRenderer();
			int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
			for (int j = 0; j < blendShapeCount; j++)
			{
                skinnedMeshRenderer.SetBlendShapeWeight(j, 0f);
            }
		}
		fBSTarget = human.face.mouthCtrl.FBSTarget;
		for (int i = 0; i < fBSTarget.Length; i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer2 = fBSTarget[i].GetSkinnedMeshRenderer();
			int blendShapeCount2 = skinnedMeshRenderer2.sharedMesh.blendShapeCount;
			for (int k = 0; k < blendShapeCount2; k++)
			{
				skinnedMeshRenderer2.SetBlendShapeWeight(k, 0f);
			}
		}
		fBSTarget = human.face.eyebrowCtrl.FBSTarget;
		for (int i = 0; i < fBSTarget.Length; i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer3 = fBSTarget[i].GetSkinnedMeshRenderer();
			int blendShapeCount3 = skinnedMeshRenderer3.sharedMesh.blendShapeCount;
			for (int l = 0; l < blendShapeCount3; l++)
			{
				skinnedMeshRenderer3.SetBlendShapeWeight(l, 0f);
			}
		}
	}

	private void CreateMorph()
	{
        Human human = SVSExporterPlugin.selectedChara;

        //Eye
        var fBSTarget = human.face.eyesCtrl.FBSTarget;
		

		for(int i = 0;i < fBSTarget.Length; i++)
		{
			var smr = fBSTarget[i].GetSkinnedMeshRenderer();
			var mesh = smr.sharedMesh;

			if (!vertexCountRecord.TryGetValue(smr.name + smr.sharedMaterial.name, out int num))
			{
				continue;
			}

            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> array = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.vertices.Length);
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> deltaNormals = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.normals.Length);
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> deltaTangents = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.tangents.Length);

            for (int j = 0; j < mesh.blendShapeCount; j++)
			{

				var morphName = mesh.GetBlendShapeName(j);
				bool flag = false;
				for (int k = 0; k < pmxFile.MorphList.Count; k++)
				{
                    if (pmxFile.MorphList[k].Name.Equals(morphName))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}

                PmxMorph pmxMorph = new PmxMorph
				{
					Name = morphName,
					NameE = "",
					Panel = 1,
					Kind = PmxMorph.OffsetKind.Vertex
				};

				mesh.GetBlendShapeFrameVertices(j, 0, array, deltaNormals, deltaTangents);
				for(int k = 0;k < array.Length; k++)
				{
					PmxVertexMorph pmxVertexMorph = new PmxVertexMorph(num + k, new PmxLib.Vector3(0f - array[k].x, array[k].y, 0f - array[k].z));
                    pmxMorph.OffsetList.Add(pmxVertexMorph);

                }
				pmxFile.MorphList.Add(pmxMorph);
			}
        }

        // Mouth
        fBSTarget = human.face.mouthCtrl.FBSTarget;

        for (int i = 0; i < fBSTarget.Length; i++)
        {
            var smr = fBSTarget[i].GetSkinnedMeshRenderer();
			var mesh = smr.sharedMesh;

            if (!vertexCountRecord.TryGetValue(smr.name + smr.sharedMaterial.name, out int num))
            {
                continue;
            }

            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> array = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.vertices.Length);
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> deltaNormals = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.normals.Length);
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> deltaTangents = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.tangents.Length);

            for (int j = 0; j < mesh.blendShapeCount; j++)
            {

                var morphName = mesh.GetBlendShapeName(j);
                bool flag = false;
                for (int k = 0; k < pmxFile.MorphList.Count; k++)
                {
                    if (pmxFile.MorphList[k].Name.Equals(morphName))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    continue;
                }

                PmxMorph pmxMorph = new PmxMorph
                {
                    Name = morphName,
                    NameE = "",
                    Panel = 1,
                    Kind = PmxMorph.OffsetKind.Vertex
                };

                mesh.GetBlendShapeFrameVertices(j, 0, array, deltaNormals, deltaTangents);
                for (int k = 0; k < array.Length; k++)
                {
                    PmxVertexMorph pmxVertexMorph = new PmxVertexMorph(num + k, new PmxLib.Vector3(0f - array[k].x, array[k].y, 0f - array[k].z));
                    pmxMorph.OffsetList.Add(pmxVertexMorph);
                }
                pmxFile.MorphList.Add(pmxMorph);
            }
        }


        // Mouth
        fBSTarget = human.face.eyebrowCtrl.FBSTarget;

        for (int i = 0; i < fBSTarget.Length; i++)
        {
            var smr = fBSTarget[i].GetSkinnedMeshRenderer();
            var mesh = smr.sharedMesh;

            if (!vertexCountRecord.TryGetValue(smr.name + smr.sharedMaterial.name, out int num))
            {
                continue;
            }

            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> array = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.vertices.Length);
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> deltaNormals = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.normals.Length);
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3> deltaTangents = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<UnityEngine.Vector3>(mesh.tangents.Length);

            for (int j = 0; j < mesh.blendShapeCount; j++)
            {

                var morphName = mesh.GetBlendShapeName(j);
                bool flag = false;
                for (int k = 0; k < pmxFile.MorphList.Count; k++)
                {
                    if (pmxFile.MorphList[k].Name.Equals(morphName))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    continue;
                }

                PmxMorph pmxMorph = new PmxMorph
                {
                    Name = morphName,
                    NameE = "",
                    Panel = 1,
                    Kind = PmxMorph.OffsetKind.Vertex
                };

                mesh.GetBlendShapeFrameVertices(j, 0, array, deltaNormals, deltaTangents);
                for (int k = 0; k < array.Length; k++)
                {
                    PmxVertexMorph pmxVertexMorph = new PmxVertexMorph(num + k, new PmxLib.Vector3(0f - array[k].x, array[k].y, 0f - array[k].z));
                    pmxMorph.OffsetList.Add(pmxVertexMorph);
                }
                pmxFile.MorphList.Add(pmxMorph);
            }
        }

		GameObject.Destroy(GameObject.Find("Morph info carrier"));
    }

	public void CreateMaterial(Material material, string matName, int count)
	{
		PmxMaterial pmxMaterial = new PmxMaterial
		{
			Name = matName,
			NameE = matName,
			Flags = (PmxMaterial.MaterialFlags.DrawBoth | PmxMaterial.MaterialFlags.Shadow | PmxMaterial.MaterialFlags.SelfShadowMap | PmxMaterial.MaterialFlags.SelfShadow)
		};
		pmxMaterial.FaceCount = count;
		pmxFile.MaterialList.Add(pmxMaterial);
	}

	private void AddFaceList(int[] faceList, int count)
	{
		for (int i = 0; i < faceList.Length; i++)
		{
			faceList[i] += count;
			pmxFile.FaceList.Add(faceList[i]);
		}
	}

	private void AddAccessory()
	{
		MeshFilter[] componentsInChildren = GameObject.Find("BodyTop").transform.GetComponentsInChildren<MeshFilter>(includeInactive: true);
		
		SkinnedMeshRenderer meshCopier = gameObjectMeshCopier.GetComponent<SkinnedMeshRenderer>();

		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MeshRenderer meshRenderer = componentsInChildren[i].gameObject.GetComponent<MeshRenderer>();
			if (
				!meshRenderer.enabled 
				|| 
				(
					nowCoordinate < maxCoord 
					&& 
					ignoreList.Contains(meshRenderer.name, StringComparer.Ordinal) 
					//&& 
					//meshRenderer.sharedMaterials.Length > 0 
					//&& 
					//ignoreList.Contains(CleanUpName(meshRenderer.sharedMaterial.name), StringComparer.Ordinal)
				) 
				|| 
				(
					nowCoordinate == maxCoord 
					&& 
					!ignoreList.Contains(meshRenderer.name, StringComparer.Ordinal) 
					//&& 
					//meshRenderer.sharedMaterials.Length > 0 
					//&& 
					//!ignoreList.Contains(CleanUpName(meshRenderer.sharedMaterial.name), StringComparer.Ordinal)
				)
			)
			{
				continue;
			}
            meshRenders.Add(meshRenderer);
			Console.WriteLine("Exporting Acc: " + meshRenderer.name);
			SMRData sMRData = new SMRData(this, meshRenderer);
            AddToSMRDataList(sMRData);
            smrMaterialsCache.Add(sMRData.SMRPath, sMRData.SMRMaterialNames);
            MaterialDataComplete matData = new MaterialDataComplete(this, meshRenderer);
			AddToMaterialDataCompleteList(matData);
			if (currentRendererMaterialMapping.ContainsKey(meshRenderer))
			{
				Console.WriteLine("Issue - Renderer already added to Material name cache: " + sMRData.SMRName);
			}
			else
			{
				currentRendererMaterialMapping.Add(meshRenderer, sMRData.SMRMaterialNames);
			}
			GameObject gameObject = componentsInChildren[i].gameObject;
			Mesh mesh = new Mesh();

			if (componentsInChildren[i].mesh != null)
			{
				meshCopier.sharedMesh = componentsInChildren[i].mesh;
			}
			else
			{
				meshCopier.sharedMesh = componentsInChildren[i].sharedMesh;
			}
			meshCopier.BakeMesh(mesh);
			meshCopier.sharedMesh = null;

			//_ = sharedMesh.boneWeights;
			Transform transform = componentsInChildren[i].gameObject.transform;
			int bone = sbi(GetAltBoneName(transform), transform.GetInstanceID().ToString());
			UnityEngine.Vector2[] uv = mesh.uv;
			List<UnityEngine.Vector2[]> list = new List<UnityEngine.Vector2[]> { mesh.uv2, mesh.uv3, mesh.uv4 };
			UnityEngine.Vector3[] normals = mesh.normals;
			UnityEngine.Vector3[] vertices = mesh.vertices;
			for (int j = 0; j < mesh.subMeshCount; j++)
			{
				int[] triangles = mesh.GetTriangles(j);
				AddFaceList(triangles, vertexCount);
				CreateMaterial(meshRenderer.sharedMaterials[j], sMRData.SMRMaterialNames[j], triangles.Length);
			}
			vertexCount += mesh.vertexCount;
			bool uv_error_flag = false;
			for (int k = 0; k < mesh.vertexCount; k++)
			{
				PmxVertex pmxVertex = new PmxVertex();
				try {
					pmxVertex.UV = new PmxLib.Vector2(uv[k].x, (float)((double)(0f - uv[k].y) + 1.0));
					pmxVertex.Weight = new PmxVertex.BoneWeight[4];
				}
				catch {
					if (uv_error_flag == false)
					{
						Console.WriteLine("Issue - Object did not have a UV map: " + meshRenderer.name);
					}
					uv_error_flag = true;
					pmxVertex.UV = new PmxLib.Vector2();
					pmxVertex.Weight = new PmxVertex.BoneWeight[4];
				}
				pmxVertex.Weight[0].Bone = bone;
				pmxVertex.Weight[0].Value = 1f;
				for (int l = 0; l < list.Count; l++)
				{
					if (list[l].Length != 0)
					{
						pmxVertex.UVA[l] = new PmxLib.Vector4(list[l][k].x, (float)((double)(0f - list[l][k].y) + 1.0), 0f, 0f);
					}
				}
				UnityEngine.Vector3 vector = gameObject.transform.TransformDirection(normals[k]);
				pmxVertex.Normal = new PmxLib.Vector3(0f - vector.x, vector.y, 0f - vector.z);
				UnityEngine.Vector3 vector2 = gameObject.transform.TransformPoint(vertices[k]);
				pmxVertex.Position = new PmxLib.Vector3((0f - vector2.x) * (float)scale, vector2.y * (float)scale, (0f - vector2.z) * (float)scale);
				pmxVertex.Deform = PmxVertex.DeformType.BDEF4;
				pmxFile.VertexList.Add(pmxVertex);
			}
            //ExportLightTexture(mesh, meshRenderer.materials, sMRData.SMRName, meshRenderer, sMRData.SMRMaterialNames);
            Mesh.Destroy(mesh);
        }

	}

	public void CreateBoneList()
	{
		offsetBoneCandidates.Clear();
		currentBonesList.Clear();
		Transform transform = GameObject.Find("BodyTop").transform;
		Dictionary<Transform, int> dictionary = new Dictionary<Transform, int>();
		Transform[] componentsInChildren = transform.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(GetAltBoneName(componentsInChildren[i]));
			stringBuilder.Append(" ");
			stringBuilder.Append(componentsInChildren[i].GetInstanceID().ToString());
			string text = stringBuilder.ToString();
			if (pmxFile.BoneBackupData.TryGetValue(text, out var value))
			{
				if (!whitelistOffsetBones.Contains(GetAltBoneName(componentsInChildren[i], ignoreID: true), StringComparer.Ordinal))
				{
					continue;
				}
				PmxBone pmxBone = value.PmxBone;
				if (pmxBone != null)
				{
					UnityEngine.Vector3 vector = componentsInChildren[i].transform.position * scale;
					BoneOffsetData bnOffsetData = new BoneOffsetData(offset: new PmxLib.Vector3(0f - vector.x, vector.y, 0f - vector.z) - pmxBone.Position, boneName: GetAltBoneName(componentsInChildren[i]));
					AddToBoneOffsetDataList(bnOffsetData);
					offsetBoneCandidates.Add(text);
					StringBuilder stringBuilder2 = new StringBuilder();
					stringBuilder2.Append(GetAltBoneName(componentsInChildren[i]));
					stringBuilder2.Append(" ");
					stringBuilder2.Append(componentsInChildren[i].GetInstanceID().ToString());
					text = stringBuilder2.ToString();
				}
			}
			int value2;
			if (pmxFile.BoneList.Count > 0)
			{
				StringBuilder stringBuilder3 = new StringBuilder();
				stringBuilder3.Append(GetAltBoneName(componentsInChildren[i].parent));
				stringBuilder3.Append(" ");
				stringBuilder3.Append(componentsInChildren[i].parent.GetInstanceID().ToString());
				string key = stringBuilder3.ToString();
				currentBoneKeysList.TryGetValue(key, out value2);
			}
			else
			{
				dictionary.TryGetValue(componentsInChildren[i].parent, out value2);
			}
			PmxBone pmxBone2 = new PmxBone
			{
				Name = GetAltBoneName(componentsInChildren[i]),
				NameE = componentsInChildren[i].GetInstanceID().ToString(),
				Parent = value2
			};
			UnityEngine.Vector3 vector2 = componentsInChildren[i].transform.position * scale;
			pmxBone2.Position = new PmxLib.Vector3(0f - vector2.x, vector2.y, 0f - vector2.z);
			dictionary.Add(componentsInChildren[i], i);
			pmxFile.BoneList.Add(pmxBone2);
			pmxFile.BoneBackupData.Add(text, new Pmx.BackupBoneData(pmxBone2.Name, componentsInChildren[i].GetInstanceID(), pmxBone2));
			currentBoneKeysList.Add(text, currentBoneKeysList.Count);
			currentBonesList.Add(pmxBone2.Name);
            editBoneInfo.Add(new BoneInfo(pmxBone2.Name, componentsInChildren[i]));
        }
        foreach (BoneInfo boneInfo in finalBoneInfoCache)
        {
			string name = GetAltBoneName(boneInfo.targetTransform);

            if (!finalBoneInfo.ContainsKey(name))
			{
				finalBoneInfo.Add(name, boneInfo.CreateNew(name));
			}
            //boneInfo.rename(GetAltBoneName(boneInfo.targetTransform)); // Update the bone name to match the exported data
        }
		finalBoneInfoCache.Clear();
    }

	private int sbi(string name, string id)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(name);
		stringBuilder.Append(" ");
		stringBuilder.Append(id);
		string text = stringBuilder.ToString();
		if (!currentBoneKeysList.TryGetValue(text, out var value))
		{
			Console.WriteLine("SBI Failed for: " + text);
		}
		return value;
	}

	//   public void SaveBodyTextures()
	//{
	//	List<string> list = new List<string> { "_AM", "_MT" };
	//	try
	//	{
	//		ChaControl characterControl = MakerAPI.GetCharacterControl();
	//		foreach (string item in list)
	//		{
	//			string text = typeMap[item];
	//			Material matDraw = characterControl.customTexCtrlBody.matDraw;
	//			Texture texture = matDraw.GetTexture(text);
	//			if (texture == null)
	//			{
	//				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
	//				texture2D.SetPixel(0, 0, new Color(1f, 1f, 1f, 1f));
	//				texture2D.Apply();
	//				texture = texture2D;
	//			}
	//			if (texture != null)
	//			{
	//				WriteToTexture2D(matDraw, text, baseSavePath + "cf_m_body" + item + "_" + nowCoordinate.ToString("00") + ".png", texture);
	//			}
	//		}
	//	}
	//	catch (Exception value)
	//	{
	//		Console.WriteLine(value);
	//	}
	//}

	//public void SaveHeadTextures()
	//{
	//	List<string> list = new List<string> { "_MT" };
	//	List<string> list2 = new List<string> { "cf_m_face_tex", "cf_m_hitomi_00_cf_Ohitomi_L02", "cf_m_hitomi_00_cf_Ohitomi_R02" };
	//	try
	//	{
	//		ChaControl characterControl = MakerAPI.GetCharacterControl();
	//		List<Material> list3 = new List<Material>
	//		{
	//			characterControl.customMatFace,
	//			characterControl.rendEye[0].material,
	//			characterControl.rendEye[1].material
	//		};
	//		Console.WriteLine(characterControl.rendEye[0].name);
	//		Console.WriteLine(characterControl.rendEye[1].name);
	//		for (int i = 0; i < list3.Count; i++)
	//		{
	//			if (list3[i] == null)
	//			{
	//				continue;
	//			}
	//			foreach (string item in list)
	//			{
	//				string text = typeMap[item];
	//				Texture texture = list3[i].GetTexture(text);
	//				if (texture == null)
	//				{
	//					Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
	//					texture2D.SetPixel(0, 0, new Color(1f, 1f, 1f, 1f));
	//					texture2D.Apply();
	//					texture = texture2D;
	//				}
	//				if (texture != null)
	//				{
	//					WriteToTexture2D(list3[i], text, baseSavePath + list2[i] + item + "_" + nowCoordinate.ToString("00") + ".png", texture);
	//				}
	//			}
	//		}
	//	}
	//	catch (Exception value)
	//	{
	//		Console.WriteLine(value);
	//	}
	//}

	//public void ExportSpecialTextures()
	//{
	//	try
	//	{
	//		ChaControl characterControl = MakerAPI.GetCharacterControl();
	//		if (nowCoordinate == maxCoord)
	//		{
	//			Material matCreate = ((CustomTextureCreate)characterControl.customTexCtrlBody).matCreate;
	//			string text = CleanUpMaterialName(matCreate.name);
	//			text = text.Substring(0, text.Length - 7);
	//			foreach (string item in new List<string> { "_CM", "_T3", "_T4", "_T6" })
	//			{
	//				string text2 = typeMap[item];
	//				if (matCreate.HasProperty(text2))
	//				{
	//					Texture texture = matCreate.GetTexture(text2);
	//					if (texture != null)
	//					{
	//						WriteToTexture2D(matCreate, text2, savePath + text + item + ".png", texture);
	//					}
	//				}
	//			}
	//			Texture texMain = ((CustomTextureCreate)characterControl.customTexCtrlBody).texMain;
	//			if (texMain != null)
	//			{
	//				WriteToTexture2D(((CustomTextureCreate)characterControl.customTexCtrlBody).matCreate, "_MainTex", savePath + text + "_MT.png", texMain);
	//			}
	//			Material matCreate2 = ((CustomTextureCreate)characterControl.customTexCtrlFace).matCreate;
	//			string text3 = CleanUpMaterialName(matCreate2.name);
	//			text3 = text3.Substring(0, text3.Length - 7) + "_00";
	//			foreach (string item2 in new List<string> { "_CM", "_T3", "_T4", "_T5", "_T6", "_T7" })
	//			{
	//				string text4 = typeMap[item2];
	//				if (matCreate2.HasProperty(text4))
	//				{
	//					Texture texture2 = matCreate2.GetTexture(text4);
	//					if (texture2 != null)
	//					{
	//						WriteToTexture2D(matCreate2, text4, savePath + text3 + item2 + ".png", texture2);
	//					}
	//				}
	//			}
	//			Texture texMain2 = ((CustomTextureCreate)characterControl.customTexCtrlFace).texMain;
	//			if (texMain2 != null)
	//			{
	//				WriteToTexture2D(((CustomTextureCreate)characterControl.customTexCtrlFace).matCreate, "_MainTex", savePath + text3 + "_MT.png", texMain2);
	//			}
	//		}
	//		if (nowCoordinate < maxCoord)
	//		{
	//			ExportClothingTextures(characterControl.ctCreateClothes, characterControl.cusClothesCmp, debug: false);
	//			ExportClothingTextures(characterControl.ctCreateClothesSub, characterControl.cusClothesSubCmp, debug: false);
	//		}
	//	}
	//	catch (Exception value)
	//	{
	//		Console.WriteLine(value);
	//	}
	//}

	//public void ExportClothingTextures(CustomTextureCreate[,] ctCloths, ChaClothesComponent[] cusCloths, bool debug)
	//{
	//	for (int i = 0; i < ctCloths.GetLength(0); i++)
	//	{
	//		for (int j = 0; j < ctCloths.GetLength(1); j++)
	//		{
	//			CustomTextureCreate customTextureCreate = ctCloths[i, j];
	//			if (customTextureCreate == null)
	//			{
	//				continue;
	//			}
	//			List<string> list = new List<string>();
	//			ChaClothesComponent chaClothesComponent = cusCloths[i];
	//			List<Renderer[]> list2;
	//			try
	//			{
	//				list2 = GetClothRenderers(chaClothesComponent);
	//			}
	//			catch (Exception)
	//			{
	//				list2 = new List<Renderer[]> { chaClothesComponent.rendNormal01, chaClothesComponent.rendNormal02 };
	//			}
	//			try
	//			{
	//				Renderer[] array = list2[j];
	//				foreach (Renderer renderer in array)
	//				{
	//					if (renderer != null)
	//					{
	//						string name = renderer.material.name;
	//						name = CleanUpMaterialName(name);
	//						name = name + " " + GetAltInstanceID(renderer.transform.parent.gameObject);
	//						list.Add(name);
	//					}
	//				}
	//			}
	//			catch (Exception)
	//			{
	//			}
	//			if (chaClothesComponent.rendAccessory != null)
	//			{
	//				string name2 = chaClothesComponent.rendAccessory.sharedMaterial.name;
	//				name2 = CleanUpMaterialName(name2);
	//				name2 = name2 + " " + GetAltInstanceID(chaClothesComponent.rendAccessory.transform.parent.gameObject);
	//				list.Add(name2);
	//			}
	//			if (list.Count == 0)
	//			{
	//				string item = "NotFound_" + customTextureCreate.texMain.name.Substring(0, customTextureCreate.texMain.name.Length - 2);
	//				list.Add(item);
	//			}
	//			foreach (string item2 in list)
	//			{
	//				Material matCreate = customTextureCreate.matCreate;
	//				foreach (string item3 in new List<string> { "_CM", "_PM1", "_PM2", "_PM3" })
	//				{
	//					string text = typeMap[item3];
	//					if (matCreate.HasProperty(text))
	//					{
	//						Texture texture = matCreate.GetTexture(text);
	//						if (texture != null && !debug)
	//						{
	//							WriteToTexture2D(matCreate, text, savePath + item2 + item3 + ".png", texture);
	//						}
	//					}
	//				}
	//				Texture texMain = customTextureCreate.texMain;
	//				if (texMain != null && !debug)
	//				{
	//					WriteToTexture2D(customTextureCreate.matCreate, "_MainTex", savePath + item2 + "_MT.png", texMain);
	//				}
	//			}
	//		}
	//	}
	//}

	public void ExportGagEyes()
	{
		string[] names = new string[]
		{
            "cf_t_gageye_00", "cf_t_gageye_01", "cf_t_gageye_02", "cf_t_gageye_03", "cf_t_gageye_04", "cf_t_gageye_05",
            "cf_t_gageye_06", "cf_t_gageye_07", "cf_t_gageye_08", "cf_t_gageye_09", "cf_t_gageye_10",
            "cf_t_expression_00", "cf_t_expression_01"
        };
		var loadedBundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
		foreach (var bundle in loadedBundles)
		{
			if (bundle.name.Contains("mt_eye_000_00"))
			{
				foreach (var i in bundle.LoadAllAssets())
				{
					if (names.Contains(i.name))
					{
						Texture2D texture = i.TryCast<Texture2D>();
						TextureSaver.SaveTexture(texture, savePath + i.name + ".png");
						Texture2D.Destroy(texture);
					}
					UnityEngine.Object.Destroy(i);
				}
            }
		}
        //foreach (var loadedBundle in loadedBundles)
        //{
        //    if (loadedBundle.name.Contains("mt_eye_000_00"))
        //    {
        //        // Use the already loaded bundle
        //        bundle = loadedBundle;
        //        return;
        //    }
        //}
        //      var fs = new Il2CppSystem.IO.FileStream("abdata/chara/mt_eye_000_00.unity3d", Il2CppSystem.IO.FileMode.Open, Il2CppSystem.IO.FileAccess.Read);
        //      var bundle = AssetBundle.LoadFromStream(fs);

        //      foreach (var i in bundle.LoadAllAssets())
        //      {
        //	if (names.Contains(i.name))
        //	{
        //		Texture2D texture = i.TryCast<Texture2D>();
        //		TextureSaver.SaveTexture(texture, savePath +  i.name + ".png");
        //		Texture2D.Destroy(texture);
        //	}
        //      }
        //bundle.Unload(false);
        //      fs.Close();
    }

	//public void GetCreateBodyMaterials()
	//{
	//	ChaControl characterControl = MakerAPI.GetCharacterControl();
	//	MaterialData matData = new MaterialData(((CustomTextureCreate)characterControl.customTexCtrlBody).matCreate, "cf_m_body_create");
	//	AddToMaterialDataList(matData);
	//	// The body material name changes with the gender, so change the material name or some colors won't be available in the Complete json
	//	AddCreateBodyMaterialsToComplete(((CustomTextureCreate)characterControl.customTexCtrlBody).matCreate, characterControl.sex == 0 ? "cm_m_body" : "cf_m_body"); 

	//       MaterialData matData2 = new MaterialData(((CustomTextureCreate)characterControl.customTexCtrlFace).matCreate, "cf_m_face_create");
	//	AddToMaterialDataList(matData2);
	//	AddCreateBodyMaterialsToComplete(((CustomTextureCreate)characterControl.customTexCtrlFace).matCreate, "cf_m_face_00");

	//       MaterialData matData3 = new MaterialData(characterControl.ctCreateEyeW.matCreate, "cf_m_eyewhite_create");
	//	AddToMaterialDataList(matData3);
	//	AddCreateBodyMaterialsToComplete(characterControl.ctCreateEyeW.matCreate, "cf_m_sirome_00");

	//	MaterialData matData4 = new MaterialData(characterControl.ctCreateEyeL.matCreate, "cf_m_eye_create_L");
	//	AddToMaterialDataList(matData4);
	//	AddCreateBodyMaterialsToComplete(characterControl.ctCreateEyeL.matCreate, "cf_Ohitomi_L02");

	//       MaterialData matData5 = new MaterialData(characterControl.ctCreateEyeR.matCreate, "cf_m_eye_create_R");
	//	AddToMaterialDataList(matData5);
	//	AddCreateBodyMaterialsToComplete(characterControl.ctCreateEyeR.matCreate, "cf_Ohitomi_R02");
	//   }

	//public void AddCreateBodyMaterialsToComplete(Material mat, String name)
	//	{
	//		//Loop through the existing MaterialDataComplete material list to find what material to append the create information to
	//           MaterialInfo matDataCreate = new MaterialInfo(mat, name);
	//           foreach (MaterialDataComplete data in materialDataComplete)
	//		{
	//			foreach (MaterialInfo infoData in data.MatInfo)
	//			{
	//                   if (infoData.MaterialName.Contains(matDataCreate.MaterialName))
	//				{
	//					infoData.ShaderPropNames.AddRange(matDataCreate.ShaderPropNames);
	//                       infoData.ShaderPropColorValues.AddRange(matDataCreate.ShaderPropColorValues);
	//                   }
	//               }
	//		}
	//	}

	//public void GetCreateClothesMaterials()
	//{
	//	ChaControl characterControl = MakerAPI.GetCharacterControl();
	//	GetClothesCreateMaterials(characterControl.ctCreateClothes, characterControl.cusClothesCmp);
	//	GetClothesCreateMaterials(characterControl.ctCreateClothesSub, characterControl.cusClothesSubCmp, isSub: true);
	//}

	//public void GetClothesCreateMaterials(CustomTextureCreate[,] ctCloths, ChaClothesComponent[] cusCloths, bool isSub = false)
	//{
	//	for (int i = 0; i < ctCloths.GetLength(0); i++)
	//	{
	//		for (int j = 0; j < ctCloths.GetLength(1); j++)
	//		{
	//			CustomTextureCreate customTextureCreate = ctCloths[i, j];
	//			if (customTextureCreate == null)
	//			{
	//				continue;
	//			}
	//			List<string> list = new List<string>();
	//			Texture texMain = customTextureCreate.texMain;
	//			ChaClothesComponent chaClothesComponent = cusCloths[i];
	//			List<Renderer[]> list2;
	//			try
	//			{
	//				list2 = GetClothRenderers(chaClothesComponent);
	//			}
	//			catch (Exception)
	//			{
	//				list2 = new List<Renderer[]> { chaClothesComponent.rendNormal01, chaClothesComponent.rendNormal02 };
	//			}
	//			try
	//			{
	//				Renderer[] array = list2[j];
	//				foreach (Renderer renderer in array)
	//				{
	//					if (renderer != null)
	//					{
	//						currentRendererMaterialMapping.TryGetValue(renderer, out var value);
	//						list.AddRange(value);
	//					}
	//				}
	//			}
	//			catch (Exception)
	//			{
	//			}
	//			if (chaClothesComponent.rendAccessory != null)
	//			{
	//				currentRendererMaterialMapping.TryGetValue(chaClothesComponent.rendAccessory, out var value2);
	//				list.AddRange(value2);
	//			}
	//			if (list.Count == 0)
	//			{
	//				string item = "NotFound_" + texMain.name.Substring(0, texMain.name.Length - 2);
	//				list.Add(item);
	//			}
	//			for (int l = 0; l < list.Count; l++)
	//			{
	//				MaterialData matData = new MaterialData(customTextureCreate.matCreate, "create_" + list[l]);
	//				AddToMaterialDataList(matData);

	//                   //Loop through the existing MaterialDataComplete material list to find what material to append the create information to
	//                   MaterialInfo matDataCreate = new MaterialInfo(customTextureCreate.matCreate, list[l]);
	//                   foreach (MaterialDataComplete data in materialDataComplete)
	//				{
	//					foreach (MaterialInfo infoData in data.MatInfo)
	//					{
	//                           if (infoData.MaterialName.Contains(matDataCreate.MaterialName))
	//						{
	//							infoData.ShaderPropNames.AddRange(matDataCreate.ShaderPropNames);
	//                               infoData.ShaderPropColorValues.AddRange(matDataCreate.ShaderPropColorValues);
	//                           }
	//                       }
	//				}
	//               }
	//           }
	//	}
	//}

	//private List<Renderer[]> GetClothRenderers(ChaClothesComponent clothesComponent)
	//{
	//	List<Renderer[]> list = new List<Renderer[]> { clothesComponent.rendNormal01, clothesComponent.rendNormal02 };
	//	typeof(ChaClothesComponent).TryGetVariable<ChaClothesComponent, Renderer[]>("rendNormal03", clothesComponent, out var variable);
	//	if (variable != null)
	//	{
	//		list.Add(variable);
	//	}
	//	return list;
	//}

	//public void CreateClothesData()
	//{
	//	ChaControl characterControl = MakerAPI.GetCharacterControl();
	//	for (int i = 0; i < characterControl.cusClothesCmp.Length; i++)
	//	{
	//           ClothesData clothData = new ClothesData("CusClothesCmp", characterControl.fileStatus.coordinateType, characterControl.cusClothesCmp[i]);
	//		AddToClothesDataList(clothData);

	//           //force the indoor shoes enum in materialDataComplete to be 999
	//		try
	//           {if ((i % 7 == 0) && (i != 0))
	//			{
	//				String indoor_shoe_smr_name = clothData.RendNormal01[0];
	//				//Loop through the existing MaterialDataComplete material list to find what smr object to change the enum of
	//				foreach (MaterialDataComplete data in materialDataComplete)
	//				{
	//					if (data.SMRName == indoor_shoe_smr_name)
	//					{
	//						data.EnumIndex = 999;
	//					}
	//				}
	//			}
	//		}
	//		//whatever
	//		catch {
	//               Console.WriteLine("Issue with detecting indoor shoes");
	//           };
	//       }
	//       for (int i = 0; i < characterControl.cusClothesSubCmp.Length; i++)
	//	{
	//           ClothesData clothData2 = new ClothesData("CusClothesSubCmp", characterControl.fileStatus.coordinateType, characterControl.cusClothesSubCmp[i]);
	//		AddToClothesDataList(clothData2);
	//       }
	//}

	//public void CreateAccessoryData()
	//{
	//	ChaControl characterControl = MakerAPI.GetCharacterControl();
	//	for (int i = 0; i < characterControl.cusAcsCmp.Length; i++)
	//	{
	//		if (!(characterControl.cusAcsCmp[i] == null))
	//		{
	//			AccessoryData accData = new AccessoryData(characterControl.fileStatus.coordinateType, characterControl.cusAcsCmp[i]);
	//			AddToAccessoryDataList(accData);
	//		}
	//	}
	//}

	//public void CreateChaFileCoordinateData()
	//{
	//	ChaFileCoordinateData coordData = new ChaFileCoordinateData(MakerAPI.GetCharacterControl());
	//	AddToChaFileCoordinateDataList(coordData);
	//}

	//public void CreateReferenceInfoData()
	//{
	//	ChaControl characterControl = MakerAPI.GetCharacterControl();
	//	foreach (int value in Enum.GetValues(typeof(ChaReference.RefObjKey)))
	//	{
	//		ReferenceInfoData refInfoData = new ReferenceInfoData(value, characterControl.GetReferenceInfo((ChaReference.RefObjKey)value));
	//		AddToReferenceInfoDataList(refInfoData);
	//	}
	//}

	public void CreateDynamicBonesData()
	{
		DynamicBone[] componentsInChildren = GameObject.Find("BodyTop").transform.GetComponentsInChildren<DynamicBone>(includeInactive: true);
		DynamicBone_Ver01[] componentsInChildren2 = GameObject.Find("BodyTop").transform.GetComponentsInChildren<DynamicBone_Ver01>(includeInactive: true);
		DynamicBone_Ver02[] componentsInChildren3 = GameObject.Find("BodyTop").transform.GetComponentsInChildren<DynamicBone_Ver02>(includeInactive: true);
		DynamicBone[] array = componentsInChildren;
		foreach (DynamicBone dynamicBone in array)
		{
			if (dynamicBone != null)
			{
				DynamicBoneData dynamicBoneData = new DynamicBoneData(dynamicBone);
				AddToDynamicBonesDataList(dynamicBoneData);
			}
		}
		DynamicBone_Ver01[] array2 = componentsInChildren2;
		foreach (DynamicBone_Ver01 dynamicBone_Ver in array2)
		{
			if (dynamicBone_Ver != null)
			{
				DynamicBoneData dynamicBoneData2 = new DynamicBoneData(dynamicBone_Ver);
				AddToDynamicBonesDataList(dynamicBoneData2);
			}
		}
		DynamicBone_Ver02[] array3 = componentsInChildren3;
		foreach (DynamicBone_Ver02 dynamicBone_Ver2 in array3)
		{
			if (dynamicBone_Ver2 != null)
			{
				DynamicBoneData dynamicBoneData3 = new DynamicBoneData(dynamicBone_Ver2);
				AddToDynamicBonesDataList(dynamicBoneData3);
			}
		}
	}

	public void CreateDynamicBoneCollidersData()
	{
		DynamicBoneCollider[] componentsInChildren = GameObject.Find("BodyTop").transform.GetComponentsInChildren<DynamicBoneCollider>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			DynamicBoneColliderData dynamicBoneColliderData = new DynamicBoneColliderData(componentsInChildren[i]);
			AddToDynamicBoneCollidersDataList(dynamicBoneColliderData);
		}
	}

	//public void CreateAccessoryStateData()
	//{
	//	ChaControl characterControl = MakerAPI.GetCharacterControl();
	//	try
	//	{
	//		CharaEvent componentInChildren = characterControl.GetComponentInChildren<CharaEvent>();
	//		if (!(componentInChildren != null))
	//		{
	//			return;
	//		}
	//		foreach (KeyValuePair<int, Slotdata> item in componentInChildren.Coordinate[nowCoordinate].Slotinfo)
	//		{
	//			AccessoryStateData accStateData = new AccessoryStateData(item.Key, item.Value);
	//			AddToAccessoryStateDataList(accStateData);
	//		}
	//	}
	//	catch (Exception ex)
	//	{
	//		Console.WriteLine(ex.Message);
	//	}
	//}

	public void CreateCharacterInfoData()
	{
        Human human = SVSExporterPlugin.selectedChara;
        StateMiniSelection stateMiniSelection = GameObject.Find("Cvs_StateMiniWindow").GetComponent<StateMiniSelection>();
        var face = human.face;
		var eyes = face.eyesCtrl;
		var eyeMatControllerl = face.eyeLookMatCtrl[0]._eyeLR == ILLGames.Unity.Animations.EYE_LR.EYE_L ? face.eyeLookMatCtrl[0] : face.eyeLookMatCtrl[1];
		var eyeMatControllerr = face.eyeLookMatCtrl[0]._eyeLR == ILLGames.Unity.Animations.EYE_LR.EYE_R ? face.eyeLookMatCtrl[0] : face.eyeLookMatCtrl[1];

		CharacterInfoData item4 = new CharacterInfoData
		{
			Personality = human.fileParam.personality,
			VoiceRate = human.fileParam.voiceRate,
			PupilWidth = human.fileFace.pupilWidth,
			PupilHeight = human.fileFace.pupilHeight,
			PupilY = human.fileFace.pupil[0].gradOffsetY,
			HlUpX = human.fileFace.pupil[0].highlightInfos[0].x,
			HlUpY = human.fileFace.pupil[0].highlightInfos[0].y,
			HlDownX = human.fileFace.pupil[0].highlightInfos[1].x,
			HlDownY = human.fileFace.pupil[0].highlightInfos[1].y,
			ShapeInfoFace = human.fileFace.shapeValueFace.ToList(),
			ShapeInfoBody = human.fileBody.shapeValueBody.ToList()
		};

        List<BlendShapeinfo> blendShapeinfos = new();

        var _ = Human.lstCtrl.GetCategoryInfo(ChaListDefine.CategoryNo.cha_eyeset);
		List<string> ptnNames = new();

		foreach (var name in _.Values)
		{
			ptnNames.Add(name.Name);
		}
		
        face.ChangeEyesOpenMax(1);
        face.ChangeEyesShaking(false);
        face.ChangeEyesBlinkFlag(false);
        face.ChangeEyesPtn(1, false);

        for (int j = 0; j < ptnNames.Count; j++)
        {
            BlendShapeinfo bld = new BlendShapeinfo(ptnNames[j]);
            blendShapeinfos.Add(bld);
            face.ChangeEyesPtn(j, false);
			face.fbsCtrl.OnLateUpdate(0);
            eyes.CalculateBlendShape(0);

            foreach (var __ in eyes.FBSTarget)
            {
                var smr = __.GetSkinnedMeshRenderer();
                for (int k = 0; k < smr.sharedMesh.blendShapeCount; k++)
                {
                    float weight = smr.GetBlendShapeWeight(k);
                    if (weight > 0)
                    {
                        bld.Add(smr.sharedMesh.GetBlendShapeName(k), weight);
                    }
                }
            }
        }
        face.ChangeEyesPtn(0, false);

        UnityEngine.Vector2 _vecl = eyeMatControllerl.material.GetTextureOffset("_Pipil_texture");
		UnityEngine.Vector2 _vecr = eyeMatControllerr.material.GetTextureOffset("_Pipil_texture");
		item4.pupilOffset = new List<float>() { _vecl.x, _vecl.y, _vecr.x, _vecr.y };

		_vecl = eyeMatControllerl.material.GetTextureOffset("_Highlight_texture_01");
		_vecr = eyeMatControllerr.material.GetTextureOffset("_Highlight_texture_01");
		item4.highlight01Offset = new List<float>() { _vecl.x, _vecl.y, _vecr.x, _vecr.y };

		_vecl = eyeMatControllerl.material.GetTextureOffset("_Highlight_texture_02");
		_vecr = eyeMatControllerr.material.GetTextureOffset("_Highlight_texture_02");
		item4.highlight02Offset = new List<float>() { _vecl.x, _vecl.y, _vecr.x, _vecr.y };

        _vecl = eyeMatControllerl.material.GetTextureOffset("_Highlight_texture_03");
        _vecr = eyeMatControllerr.material.GetTextureOffset("_Highlight_texture_03");
        item4.highlight03Offset = new List<float>() { _vecl.x, _vecl.y, _vecr.x, _vecr.y };

        _vecl = eyeMatControllerl.material.GetTextureScale("_Pipil_texture");
		_vecr = eyeMatControllerr.material.GetTextureScale("_Pipil_texture");
		item4.pupilScale = new List<float>() { _vecl.x, _vecl.y, _vecr.x, _vecr.y };

		_vecl = eyeMatControllerl.material.GetTextureScale("_Highlight_texture_01");
		_vecr = eyeMatControllerr.material.GetTextureScale("_Highlight_texture_01");
		item4.highlight01Scale = new List<float>() { _vecl.x, _vecl.y, _vecr.x, _vecr.y };

		_vecl = eyeMatControllerl.material.GetTextureScale("_Highlight_texture_02");
		_vecr = eyeMatControllerr.material.GetTextureScale("_Highlight_texture_02");
		item4.highlight02Scale = new List<float>() { _vecl.x, _vecl.y, _vecr.x, _vecr.y };

		_vecl = eyeMatControllerl.material.GetTextureScale("_Highlight_texture_03");
		_vecr = eyeMatControllerr.material.GetTextureScale("_Highlight_texture_03");
		item4.highlight03Scale = new List<float>() { _vecl.x, _vecl.y, _vecr.x, _vecr.y };

		//item4.eyeRotation = new List<float>() { eyeMatControllerl._material.GetFloat("_rotation"), eyeMatControllerr._material.GetFloat("_rotation") };

		characterInfoData.Add(item4);
        ExportDataToJson(characterInfoData, "SVS_CharacterInfoData.json");
		ExportDataToJson(blendShapeinfos, "SVS_BlendShapeInfo.json");
	}

	//public void CreateListInfoData()
	//{
	//	ChaControl characterControl = MakerAPI.GetCharacterControl();
	//	ListInfoBase[] infoClothes = characterControl.infoClothes;
	//	for (int i = 0; i < infoClothes.Length; i++)
	//	{
	//		ListInfoData lstData = new ListInfoData(infoClothes[i], "InfoClothes");
	//		AddToListInfoDataList(lstData);
	//	}
	//	infoClothes = characterControl.infoParts;
	//	for (int i = 0; i < infoClothes.Length; i++)
	//	{
	//		ListInfoData lstData2 = new ListInfoData(infoClothes[i], "InfoParts");
	//		AddToListInfoDataList(lstData2);
	//	}
	//	infoClothes = characterControl.infoAccessory;
	//	for (int i = 0; i < infoClothes.Length; i++)
	//	{
	//		ListInfoData lstData3 = new ListInfoData(infoClothes[i], "InfoAccessory");
	//		AddToListInfoDataList(lstData3);
	//	}
	//	infoClothes = characterControl.infoHair;
	//	for (int i = 0; i < infoClothes.Length; i++)
	//	{
	//		ListInfoData lstData4 = new ListInfoData(infoClothes[i], "InfoHair");
	//		AddToListInfoDataList(lstData4);
	//	}
	//}

	public void AddToSMRDataList(SMRData smrData)
	{
		if (characterSMRData.Find((SMRData data) => string.CompareOrdinal(smrData.SMRName, data.SMRName) == 0 && string.CompareOrdinal(smrData.SMRPath, data.SMRPath) == 0) == null)
		{
			characterSMRData.Add(smrData);
		}
	}
    public void AddToMaterialDataCompleteList(MaterialDataComplete smrData)
    {
        if (materialDataComplete.Find((MaterialDataComplete data) => string.CompareOrdinal(smrData.SMRName, data.SMRName) == 0 && string.CompareOrdinal(smrData.SMRPath, data.SMRPath) == 0) == null)
        {
            materialDataComplete.Add(smrData);
        }
    }

    public void AddToTextureDataList(TextureData texData)
	{
		if (texData.textureName != null && textureData.Find((TextureData data) => string.CompareOrdinal(texData.textureName, data.textureName) == 0) == null)
		{
			textureData.Add(texData);
		}
	}

	//public void AddToMaterialDataList(MaterialData matData)
	//{
	//	if ((materialData.Find((MaterialData data) => string.CompareOrdinal(matData.MaterialName, data.MaterialName) == 0) == null || string.CompareOrdinal(matData.MaterialName, EyeMatName) == 0) && string.CompareOrdinal(matData.MaterialName, "Bonelyfans") != 0)
	//	{
	//		materialData.Add(matData);
	//	}
	//}

	//   public void AddToClothesDataList(ClothesData clothData)
	//{
	//	clothesData.Add(clothData);
	//}

	//public void AddToAccessoryDataList(AccessoryData accData)
	//{
	//	accessoryData.Add(accData);
	//}

	//public void AddToChaFileCoordinateDataList(ChaFileCoordinateData coordData)
	//{
	//	chaFileCoordinateData.Add(coordData);
	//}

	//public void AddToReferenceInfoDataList(ReferenceInfoData refInfoData)
	//{
	//	referenceInfoData.Add(refInfoData);
	//}

	public void AddToDynamicBonesDataList(DynamicBoneData dynamicBoneData)
	{
		dynamicBonesData.Add(dynamicBoneData);
	}

	public void AddToDynamicBoneCollidersDataList(DynamicBoneColliderData dynamicBoneColliderData)
	{
		dynamicBoneCollidersData.Add(dynamicBoneColliderData);
	}

	//public void AddToAccessoryStateDataList(AccessoryStateData accStateData)
	//{
	//	accessoryStateData.Add(accStateData);
	//}

	public void AddToBoneOffsetDataList(BoneOffsetData bnOffsetData)
	{
		boneOffsetData.Add(bnOffsetData);
	}

	//public void AddToListInfoDataList(ListInfoData lstData)
	//{
	//	listInfoData.Add(lstData);
	//}

	//public void ExportChaFileCoordinateDataListToJson(List<ChaFileCoordinateData> dataList, string fileName)
	//{
	//	string text = "";
	//	foreach (ChaFileCoordinateData data in dataList)
	//	{
	//		string text2 = JsonUtility.ToJson(data);
	//		text2 = text2.Replace("}", ",");
	//		text2 = text2 + "\"Clothes\":" + JsonUtility.ToJson(data.Clothes).Replace("}", ",");
	//		text2 += "\"Parts\":[";
	//		foreach (ChaFileClothes_PartsInfo part in data.Clothes.Parts)
	//		{
	//			text2 = text2 + JsonUtility.ToJson(part) + ",";
	//		}
	//		text2 = text2.TrimEnd(',');
	//		text2 += "]},";
	//		text2 = text2 + "\"Accessory\":" + JsonUtility.ToJson(data.Accessory).Replace("}", "");
	//		text2 += "\"Parts\":[";
	//		foreach (ChaFileAccessory_PartsInfo part2 in data.Accessory.Parts)
	//		{
	//			text2 = text2 + JsonUtility.ToJson(part2) + ",";
	//		}
	//		text2 = text2.TrimEnd(',');
	//		text2 += "]},";
	//		text2 = text2 + "\"Makeup\":" + JsonUtility.ToJson(data.Makeup);
	//		text2 += "}";
	//		text = text + text2 + ",\n";
	//	}
	//	if (!text.IsNullOrEmpty())
	//	{
	//		text = text.Substring(0, text.Length - 2);
	//	}
	//	text = "[" + text + "]";
	//	File.WriteAllText(baseSavePath + fileName, text);
	//}


	public void ExportDataToJson<T>(T data, string fileName)
	{
        string text = CustomJsonSerializer.Serialize(data);

        File.WriteAllText(savePath + fileName, text);
    }

	public static string GetGameObjectPath(GameObject obj)
	{
		string text = "/" + GetAltBoneName(obj, ignoreID: true);
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			text = "/" + GetAltBoneName(obj, ignoreID: true) + text;
		}
		return text;
	}

	public static int GetAltInstanceID(UnityEngine.Object _object)
	{
		if (instanceIDs.TryGetValue(_object.GetInstanceID(), out var value) && value >= 0)
		{
			int.TryParse(value.ToString() + nowCoordinate, out value);
			return value;
		}
		Console.WriteLine("No ID Found");
		return -1;
	}

	public static string GetAltBoneName(UnityEngine.Object _object, bool ignoreID = false)
	{
		string cleanedName = CleanUpName(_object.name);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(cleanedName);
		stringBuilder.Append(" ");
		stringBuilder.Append(_object.GetInstanceID().ToString());
		string text = stringBuilder.ToString();
		bool flag = nowCoordinate == minCoord;
		bool flag2 = false;
		bool flag3 = pmxFile.BoneBackupData.ContainsKey(text);
		if (flag3)
		{
			flag2 = offsetBoneCandidates.Contains(text, StringComparer.Ordinal);
		}
		if (flag && currentBonesList.Contains(cleanedName))
		{
			flag = false;
		}
		if (flag || ignoreID || (flag3 && !flag2))
		{
			return cleanedName;
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append(cleanedName);
		stringBuilder2.Append(" ");
		stringBuilder2.Append(GetAltInstanceID(_object).ToString());
		return stringBuilder2.ToString();
	}

	public static string GetAltMaterialName(PmxBuilder pmxBuilder, string materialName)
	{
		if (currentMaterialList.TryGetValue(materialName, out var value))
		{
			if (!pmxBuilder.ignoreList.Contains(materialName))
			{
				currentMaterialList[materialName] = value + 1;
				materialName = materialName + " " + value.ToString("00");
			}
		}
		else
		{
			currentMaterialList.Add(materialName, value);
		}
		return materialName;
	}

	public void CreatePmxHeader()
	{
		PmxElementFormat pmxElementFormat = new PmxElementFormat(1f)
		{
			VertexSize = PmxElementFormat.GetUnsignedBufSize(pmxFile.VertexList.Count),
			UVACount = 3
		};
		int val = int.MinValue;
		for (int i = 0; i < pmxFile.BoneList.Count; i++)
		{
			val = Math.Max(val, Math.Abs(pmxFile.BoneList[i].IK.LinkList.Count));
		}
		int count = Math.Max(val, pmxFile.BoneList.Count);
		pmxElementFormat.BoneSize = PmxElementFormat.GetSignedBufSize(count);
		if (pmxElementFormat.BoneSize < 2)
		{
			pmxElementFormat.BoneSize = 2;
		}
		pmxElementFormat.MorphSize = PmxElementFormat.GetUnsignedBufSize(pmxFile.MorphList.Count);
		pmxElementFormat.MaterialSize = PmxElementFormat.GetUnsignedBufSize(pmxFile.MaterialList.Count);
		pmxElementFormat.BodySize = PmxElementFormat.GetUnsignedBufSize(pmxFile.BodyList.Count);
		PmxHeader pmxHeader = new PmxHeader(2.1f);
		pmxHeader.FromElementFormat(pmxElementFormat);
		pmxFile.Header = pmxHeader;
	}

	public void Save()
	{
		pmxFile.ToFile(currentSavePath + "model.pmx");
	}

	public void ExportAllDataLists()
	{
		ExportDataToJson(characterSMRData, "SVS_SMRData.json");
        ExportDataToJson(materialDataComplete, "SVS_MaterialDataComplete.json");
        //ExportDataToJson(materialData, "KK_MaterialData.json");
        //ExportDataToJson(textureData, "KK_TextureData.json");
        //ExportDataListToJson(clothesData, "KK_ClothesData.json");
        //ExportDataListToJson(accessoryData, "KK_AccessoryData.json");
        //ExportDataListToJson(referenceInfoData, "KK_ReferenceInfoData.json");
        ExportDataToJson(dynamicBonesData, "SVS_DynamicBoneData.json");
        ExportDataToJson(dynamicBoneCollidersData, "SVS_DynamicBoneColliderData.json");
		//ExportDataListToJson(accessoryStateData, "KK_AccessoryStateData.json");
		ExportDataToJson(boneOffsetData, "SVS_BoneOffsetData.json");
        //ExportDataToJson(listInfoData, "KK_ListInfoData.json");
        //ExportChaFileCoordinateDataListToJson(chaFileCoordinateData, "KK_ChaFileCoordinateData.json");
        ExportDataToJson(editBoneInfo, "SVS_EditBoneInfo.json");
        ExportDataToJson(finalBoneInfo.Values.ToList(), "SVS_FinalBoneInfo.json");
        ExportDataToJson(uvAdjustments, "SVS_UVAdjustments.json");
    }

	public void OpenFolderInExplorer(string filename)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		try
		{
			filename = Path.GetFullPath(filename);
			Process.Start("explorer.exe", filename ?? "");
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
		}
	}

	public static string CleanUpName(string str)
	{
		//return str.Replace("(Instance)", "").Replace("(Clone)", "").Trim();
		return str.Replace("(Instance)", "").Trim();
	}

	public static string AnimationCurveToJSON(AnimationCurve curve)
	{
		StringBuilder stringBuilder = new StringBuilder();
		BeginObject(stringBuilder);
		BeginArray(stringBuilder, "keys");
		Keyframe[] keys = curve.GetKeys();
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[i];
			BeginObject(stringBuilder);
			WriteFloat(stringBuilder, "time", keyframe.time);
			Next(stringBuilder);
			WriteFloat(stringBuilder, "value", keyframe.value);
			Next(stringBuilder);
			WriteFloat(stringBuilder, "intangent", keyframe.inTangent);
			Next(stringBuilder);
			WriteFloat(stringBuilder, "outtangent", keyframe.outTangent);
			EndObject(stringBuilder);
			if (i < keys.Length - 1)
			{
				Next(stringBuilder);
			}
		}
		EndArray(stringBuilder);
		EndObject(stringBuilder);
		return stringBuilder.ToString();
	}

	public static UnityEngine.Vector2[] GenerateUV(UnityEngine.Vector3[] vertices, int[] triangles)
	{
		UnityEngine.Vector2[] uv = new UnityEngine.Vector2[vertices.Length];
		

		return uv;
	}

	public static void BeginObject(StringBuilder sb)
	{
		sb.Append("{ ");
	}

	public static void EndObject(StringBuilder sb)
	{
		sb.Append(" }");
	}

	public static void BeginArray(StringBuilder sb, string keyname)
	{
		sb.AppendFormat("\"{0}\" : [", keyname);
	}

	public static void EndArray(StringBuilder sb)
	{
		sb.Append(" ]");
	}

	public static void WriteString(StringBuilder sb, string key, string value)
	{
		sb.AppendFormat("\"{0}\" : \"{1}\"", key, value);
	}

	public static void WriteFloat(StringBuilder sb, string key, float val)
	{
		sb.AppendFormat("\"{0}\" : {1}", key, val);
	}

	public static void WriteInt(StringBuilder sb, string key, int val)
	{
		sb.AppendFormat("\"{0}\" : {1}", key, val);
	}

	public static void WriteVector3(StringBuilder sb, string key, UnityEngine.Vector3 val)
	{
		sb.AppendFormat("\"{0}\" : {1}", key, CustomJsonSerializer.Serialize(val));
	}

	public static void Next(StringBuilder sb)
	{
		sb.Append(", ");
	}
}
