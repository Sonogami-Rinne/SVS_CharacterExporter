import pathlib
import UnityPy
from UnityPy import *
from UnityPy.resources import *


files = []
for file in pathlib.Path('.').glob('ao_*.unity3d'):
    files.append(file)
for file in pathlib.Path('.\\MOD').rglob('*.unity3d'):
    files.append(file)

length = len(files)
for file in files:
    length -= 1
    print(f'\nProcessing bundle {file.name} ...')
    env = UnityPy.load(str(file))
    ress_dict = {}
    modify_flag = False
    for cab in env.cabs:
        if cab.endswith('.ress'):
            ress_dict[cab.lower()] = env.cabs[cab].bytes

    for asset in env.objects:
        if asset.type.name == 'MeshFilter':
            try:
                _env = None
                mesh = asset.read().m_Mesh
                if mesh.m_PathID == 0:
                    continue

                if mesh.m_FileID != 0:
                    target_cab = asset.assets_file.externals[mesh.m_FileID - 1].name.lower()
                    if target_cab == 'unity default resources':
                        print("\033[93m using mesh from unity default resource, ignored\033[0m")
                    else:
                        print("\033[91mUsing mesh that is from external bundles, ignored\033[0m")
                    continue
                else:
                    mesh = mesh.read()
                print(f'Found mesh {mesh.m_Name}')
                mesh_asset = mesh.object_reader
                raw_dict = mesh_asset.read_typetree()
                stream_info = mesh.m_StreamData

                raw_dict['m_IsReadable'] = True
                raw_dict['m_KeepVertices'] = True
                raw_dict['m_KeepIndices'] = True
                # raw_dict['m_MeshUsageFlags'] = 1
                if stream_info.path != '':
                    stream_data = ress_dict[(stream_info.path[stream_info.path.rindex('/') + 1:]).lower()][
                                  stream_info.offset:stream_info.offset + stream_info.size]
                    raw_dict['m_VertexData']['m_DataSize'] = stream_data
                    raw_dict['m_StreamData']['offset'] = 0
                    raw_dict['m_StreamData']['size'] = 0
                    raw_dict['m_StreamData']['path'] = ''
                mesh_asset.save_typetree(raw_dict)
                if _env is None:
                    modify_flag = True
                else:
                    _env.save('lz4', str(_env.path))
            except:
                print("\033[91mAn error occurred, ignored\033[0m")
    if modify_flag:
        print(f'Saving... remains {length}')
        env.save('lz4', str(file.parent))
