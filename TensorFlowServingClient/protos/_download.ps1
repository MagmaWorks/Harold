wget https://raw.githubusercontent.com/tensorflow/serving/master/tensorflow_serving/apis/prediction_service.proto   -OutFile .\prediction_service.proto

wget https://raw.githubusercontent.com/tensorflow/serving/master/tensorflow_serving/apis/predict.proto              -OutFile .\tensorflow_serving\apis\predict.proto
wget https://raw.githubusercontent.com/tensorflow/serving/master/tensorflow_serving/apis/model.proto                -OutFile .\tensorflow_serving\apis\model.proto
wget https://raw.githubusercontent.com/tensorflow/serving/master/tensorflow_serving/apis/input.proto                -OutFile .\tensorflow_serving\apis\input.proto
wget https://raw.githubusercontent.com/tensorflow/serving/master/tensorflow_serving/apis/classification.proto       -OutFile .\tensorflow_serving\apis\classification.proto 
wget https://raw.githubusercontent.com/tensorflow/serving/master/tensorflow_serving/apis/get_model_metadata.proto   -OutFile .\tensorflow_serving\apis\get_model_metadata.proto 
wget https://raw.githubusercontent.com/tensorflow/serving/master/tensorflow_serving/apis/get_model_status.proto     -OutFile .\tensorflow_serving\apis\get_model_status.proto 
wget https://raw.githubusercontent.com/tensorflow/serving/master/tensorflow_serving/apis/inference.proto            -OutFile .\tensorflow_serving\apis\inference.proto  
wget https://raw.githubusercontent.com/tensorflow/serving/master/tensorflow_serving/apis/regression.proto           -OutFile .\tensorflow_serving\apis\regression.proto 
wget https://raw.githubusercontent.com/tensorflow/serving/master/tensorflow_serving/util/status.proto               -OutFile .\tensorflow_serving\util\status.proto

wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/protobuf/meta_graph.proto       -OutFile .\tensorflow\core\protobuf\meta_graph.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/protobuf/saver.proto            -OutFile .\tensorflow\core\protobuf\saver.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/framework/tensor_shape.proto    -OutFile .\tensorflow\core\framework\tensor_shape.proto 
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/framework/tensor.proto          -OutFile .\tensorflow\core\framework\tensor.proto 
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/framework/types.proto           -OutFile .\tensorflow\core\framework\types.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/framework/graph.proto           -OutFile .\tensorflow\core\framework\graph.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/framework/node_def.proto        -OutFile .\tensorflow\core\framework\node_def.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/framework/function.proto        -OutFile .\tensorflow\core\framework\function.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/framework/versions.proto        -OutFile .\tensorflow\core\framework\versions.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/framework/attr_value.proto      -OutFile .\tensorflow\core\framework\attr_value.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/framework/op_def.proto          -OutFile .\tensorflow\core\framework\op_def.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/framework/resource_handle.proto -OutFile .\tensorflow\core\framework\resource_handle.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/example/example.proto           -OutFile .\tensorflow\core\example\example.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/example/feature.proto           -OutFile .\tensorflow\core\example\feature.proto
wget https://raw.githubusercontent.com/tensorflow/tensorflow/master/tensorflow/core/lib/core/error_codes.proto      -OutFile .\tensorflow\core\lib\core\error_codes.proto

wget https://raw.githubusercontent.com/google/protobuf/master/src/google/protobuf/any.proto                         -OutFile .\google\protobuf\any.proto
wget https://raw.githubusercontent.com/google/protobuf/master/src/google/protobuf/wrappers.proto                         -OutFile .\google\protobuf\wrappers.proto