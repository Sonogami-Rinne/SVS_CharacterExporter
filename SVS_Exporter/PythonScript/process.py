import pathlib
import UnityPy
from UnityPy import *
from UnityPy.resources import *


files = pathlib.Path('.').glob('ao_*.unity3d')
for file in files:
    print(f'\nProcessing bundle {file.name} ...')
    env = UnityPy.load(str(file))
    ress_dict = {}
    for cab in env.cabs:
        if cab.endswith('.ress'):
            ress_dict[cab.lower()] = env.cabs[cab].bytes

    for asset in env.objects:
        if asset.type.name == 'Mesh':
            print(f'Found mesh {asset.peek_name()}')
            stream_info = asset.read()
            stream_info = stream_info.m_StreamData

            raw_dict = asset.read_typetree()
            raw_dict['m_IsReadable'] = True
            raw_dict['m_KeepVertices'] = True
            raw_dict['m_KeepIndices'] = True
            # raw_dict['m_MeshUsageFlags'] = 1
            if stream_info.path != '':
                stream_data = ress_dict[(stream_info.path[stream_info.path.rindex('/') + 1:]).lower()][stream_info.offset:stream_info.offset + stream_info.size]
                raw_dict['m_VertexData']['m_DataSize'] = stream_data
                raw_dict['m_StreamData']['offset'] = 0
                raw_dict['m_StreamData']['size'] = 0
                raw_dict['m_StreamData']['path'] = ''
            asset.save_typetree(raw_dict)
    env.save('lz4', '.')
