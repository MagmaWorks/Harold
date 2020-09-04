FOR %%i IN (%~dp0tensorflow\core\example\*.proto) DO C:\Users\allxo_000\.nuget\packages\grpc.tools\1.10.0\tools\windows_x64\protoc.exe --proto_path="%~dp0." --csharp_out="%~dp0..\service" %%~dpni.proto
FOR %%i IN (%~dp0tensorflow\core\framework\*.proto) DO C:\Users\allxo_000\.nuget\packages\grpc.tools\1.10.0\tools\windows_x64\protoc.exe --proto_path="%~dp0." --csharp_out="%~dp0..\service" %%~dpni.proto
FOR %%i IN (%~dp0tensorflow\core\protobuf\*.proto) DO C:\Users\allxo_000\.nuget\packages\grpc.tools\1.10.0\tools\windows_x64\protoc.exe --proto_path="%~dp0." --csharp_out="%~dp0..\service" %%~dpni.proto
FOR %%i IN (%~dp0tensorflow\core\lib\core\*.proto) DO C:\Users\allxo_000\.nuget\packages\grpc.tools\1.10.0\tools\windows_x64\protoc.exe --proto_path="%~dp0." --csharp_out="%~dp0..\service" %%~dpni.proto
FOR %%i IN (%~dp0tensorflow_serving\apis\*.proto) DO C:\Users\allxo_000\.nuget\packages\grpc.tools\1.10.0\tools\windows_x64\protoc.exe --proto_path="%~dp0." --csharp_out="%~dp0..\service" %%~dpni.proto
FOR %%i IN (%~dp0tensorflow_serving\util\*.proto) DO C:\Users\allxo_000\.nuget\packages\grpc.tools\1.10.0\tools\windows_x64\protoc.exe --proto_path="%~dp0." --csharp_out="%~dp0..\service" %%~dpni.proto

C:\Users\allxo_000\.nuget\packages\grpc.tools\1.10.0\tools\windows_x64\protoc.exe -I "%~dp0." --proto_path="%~dp0." --csharp_out "%~dp0..\service"  --grpc_out "%~dp0..\service" "%~dp0prediction_service.proto" --plugin=protoc-gen-grpc="c:\Users\allxo_000\.nuget\packages\grpc.tools\1.10.0\tools\windows_x64\grpc_csharp_plugin.exe"
