The process.py is a Python script.
It process bundles to make accessory meshes readable.
Currently, this script only process bundles that original game have.
It do NOT process bundles that added by mod.

You need to use pyinstaller to turn this py file into exe.
For example: pyinstaller -F process.py --add-data "C:\Users\Administrator\AppData\Local\Packages\PythonSoftwareFoundation.Python.3.9_qbz5n2kfra8p0\LocalCache\local-packages\Python39\site-packages\UnityPy\resources;UnityPy\resources"
Adjust the parameters based on your own situation.
Or you can use python.exe to execute the script directly.

Process.py ��һ��Python�ű���
�������ض���unity3d�����ò�����Ʒ���������ɶ���
Ŀǰ������ű�ֻ������Ϸ�Դ���unity3d��
��������mod�ӵ�unity3d��

����Ҫʹ��pyinstaller���Ѹýű�����Ϊexe�ļ�
����: pyinstaller -F process.py --add-data "C:\Users\Administrator\AppData\Local\Packages\PythonSoftwareFoundation.Python.3.9_qbz5n2kfra8p0\LocalCache\local-packages\Python39\site-packages\UnityPy\resources;UnityPy\resources"
�������Լ����������������
����ֱ��ʹ��python.exe��ִ���Ǹ��ű�