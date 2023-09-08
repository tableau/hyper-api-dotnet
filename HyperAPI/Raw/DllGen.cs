// THIS IS A GENERATED FILE, DO NOT MODIFY MANUALLY
using System;
using System.Runtime.InteropServices;

namespace Tableau.HyperAPI.Raw
{
    internal static partial class Dll
    {
        private const string HyperApiDll = "tableauhyperapi";

        // hyper_error_category_t
        public const int HYPER_ERROR_CATEGORY_GENERIC = 0;
        public const int HYPER_ERROR_CATEGORY_SYSTEM = 1;
        public const int HYPER_ERROR_CATEGORY_PROCESS_EXIT_CODE = 3;
        public const int HYPER_ERROR_CATEGORY_SQLSTATE = 4;

        // hyper_error_severity_t
        public const int HYPER_SEVERITY_ERROR = 0;
        public const int HYPER_SEVERITY_FATAL = 1;
        public const int HYPER_SEVERITY_PANIC = 2;

        // hyper_error_source_t
        public const int HYPER_ERROR_SOURCE_USER = 0;
        public const int HYPER_ERROR_SOURCE_SYSTEM = 1;

        // hyper_error_field_key
        public const int HYPER_ERROR_FIELD_ERROR_CATEGORY = 0;
        public const int HYPER_ERROR_FIELD_ERROR_CODE = 1;
        public const int HYPER_ERROR_FIELD_MESSAGE = 2;
        public const int HYPER_ERROR_FIELD_HINT_MESSAGE = 3;
        public const int HYPER_ERROR_FIELD_SEVERITY = 4;
        public const int HYPER_ERROR_FIELD_SQL_STATE = 5;
        public const int HYPER_ERROR_FIELD_CAUSE = 6;
        public const int HYPER_ERROR_FIELD_CONTEXT_ID = 7;
        public const int HYPER_ERROR_FIELD_DETAIL = 8;
        public const int HYPER_ERROR_FIELD_SOURCE = 9;
        public const int HYPER_ERROR_GRPC_STATUS = 10;

        // hyper_telemetry_t
        public const int HYPER_DISABLE_TELEMETRY = 0;
        public const int HYPER_ENABLE_TELEMETRY = 1;

        // hyper_create_mode_t
        public const int HYPER_DO_NOT_CREATE = 0;
        public const int HYPER_CREATE = 1;
        public const int HYPER_CREATE_IF_NOT_EXISTS = 2;
        public const int HYPER_CREATE_AND_REPLACE = 3;

        // hyper_connection_status_t
        public const int HYPER_CONNECTION_OK = 0;
        public const int HYPER_CONNECTION_BAD = 1;

        // hyper_type_tag_t
        public const int HYPER_UNSUPPORTED = 0;
        public const int HYPER_BOOL = 1;
        public const int HYPER_BIG_INT = 2;
        public const int HYPER_SMALL_INT = 3;
        public const int HYPER_INT = 4;
        public const int HYPER_NUMERIC = 5;
        public const int HYPER_DOUBLE = 6;
        public const int HYPER_OID = 7;
        public const int HYPER_BYTE_A = 8;
        public const int HYPER_TEXT = 9;
        public const int HYPER_VARCHAR = 10;
        public const int HYPER_CHAR = 11;
        public const int HYPER_JSON = 12;
        public const int HYPER_DATE = 13;
        public const int HYPER_INTERVAL = 14;
        public const int HYPER_TIME = 15;
        public const int HYPER_TIMESTAMP = 16;
        public const int HYPER_TIMESTAMP_TZ = 17;
        public const int HYPER_GEOGRAPHY = 18;

        // hyper_table_persistence_t
        public const int HYPER_PERMANENT = 0;
        public const int HYPER_TEMPORARY = 1;

        // hyper_rowset_result_format_t
        public const int HYPER_ROWSET_RESULT_FORMAT_TEXT = 0;
        public const int HYPER_ROWSET_RESULT_FORMAT_HYPER_BINARY = 2;

        // hyper_rowset_type_t
        public const int HYPER_ROWSET_EMPTY_QUERY = 0;
        public const int HYPER_ROWSET_COMMAND = 1;
        public const int HYPER_ROWSET_QUERY_RESULT = 2;
        public const int HYPER_ROWSET_COPY_IN = 3;
        public const int HYPER_ROWSET_COPY_OUT = 4;

        // hyper_log_level_t
        public const int HYPER_LOG_LEVEL_TRACE = 0;
        public const int HYPER_LOG_LEVEL_INFO = 1;
        public const int HYPER_LOG_LEVEL_WARNING = 2;
        public const int HYPER_LOG_LEVEL_ERROR = 3;
        public const int HYPER_LOG_LEVEL_FATAL = 4;

        // anonymous enum
        public const int HYPER_OID_BOOL = 16;
        public const int HYPER_OID_BIG_INT = 20;
        public const int HYPER_OID_SMALL_INT = 21;
        public const int HYPER_OID_INT = 23;
        public const int HYPER_OID_NUMERIC = 1700;
        public const int HYPER_OID_DOUBLE = 701;
        public const int HYPER_OID_OID = 26;
        public const int HYPER_OID_BYTE_A = 17;
        public const int HYPER_OID_TEXT = 25;
        public const int HYPER_OID_VARCHAR = 1043;
        public const int HYPER_OID_CHAR = 1042;
        public const int HYPER_OID_CHAR1 = 18;
        public const int HYPER_OID_JSON = 114;
        public const int HYPER_OID_DATE = 1082;
        public const int HYPER_OID_INTERVAL = 1186;
        public const int HYPER_OID_TIME = 1083;
        public const int HYPER_OID_TIMESTAMP = 1114;
        public const int HYPER_OID_TIMESTAMP_TZ = 1184;
        public const int HYPER_OID_GEOGRAPHY = 5003;

        // typedef void (*hyper_log_function_t)(hyper_log_level_t log_level, const char* topic, const char* json_value, void* context)
        public delegate void hyper_log_function_t(int log_level, IntPtr topic, IntPtr json_value, IntPtr context);

        // hyper_error_t* hyper_error_create(uint32_t contextId)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_error_create(uint contextId);

        // void hyper_error_destroy(hyper_error_t* error)
        [DllImport(HyperApiDll)]
        public static extern void hyper_error_destroy(IntPtr error);

        // hyper_error_t* hyper_error_get_field(const hyper_error_t* error,
        //                                      hyper_error_field_key key,
        //                                      hyper_error_field_value* value)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_error_get_field(IntPtr error, int key, out hyper_error_field_value value);

        // hyper_error_t* hyper_error_set_field(hyper_error_t* error,
        //                                      hyper_error_field_key field,
        //                                      hyper_error_field_value value)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_error_set_field(IntPtr error, int field, hyper_error_field_value value);

        // void hyper_parameters_destroy(hyper_parameters_t* parameters)
        [DllImport(HyperApiDll)]
        public static extern void hyper_parameters_destroy(IntPtr parameters);

        // hyper_parameters_t* hyper_parameters_copy(const hyper_parameters_t* parameters)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_parameters_copy(IntPtr parameters);

        // hyper_error_t* hyper_parameters_set(hyper_parameters_t* parameters, const char* key, const char* value)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_parameters_set(
            IntPtr parameters,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string key,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string value);

        // hyper_error_t* hyper_create_instance_parameters(hyper_parameters_t** parameters, bool use_defaults)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_create_instance_parameters(
            out IntPtr parameters,
            [MarshalAs(UnmanagedType.I1)]
            bool use_defaults);

        // hyper_error_t* hyper_instance_create(const char* hyper_path,
        //                                      hyper_telemetry_t telemetry,
        //                                      const hyper_parameters_t* parameters,
        //                                      hyper_instance_t** instance)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_instance_create(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string hyper_path,
            int telemetry,
            IntPtr parameters,
            out IntPtr instance);

        // hyper_error_t* hyper_instance_shutdown(hyper_instance_t* instance, int timeoutMs)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_instance_shutdown(IntPtr instance, int timeoutMs);

        // void hyper_instance_close(hyper_instance_t* instance)
        [DllImport(HyperApiDll)]
        public static extern void hyper_instance_close(IntPtr instance);

        // const char* hyper_instance_get_endpoint_descriptor(const hyper_instance_t* instance)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstUtf8RetValueMarshaler))]
        public static extern string hyper_instance_get_endpoint_descriptor(IntPtr instance);

        // int64_t hyper_instance_get_pid(const hyper_instance_t* instance)
        [DllImport(HyperApiDll)]
        public static extern long hyper_instance_get_pid(IntPtr instance);

        // bool hyper_instance_get_exit_code(hyper_instance_t* instance, int* exit_code)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool hyper_instance_get_exit_code(IntPtr instance, out int exit_code);

        // hyper_error_t* hyper_create_connection_parameters(const hyper_instance_t* instance,
        //                                                   hyper_parameters_t** parameters)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_create_connection_parameters(IntPtr instance, out IntPtr parameters);

        // hyper_error_t* hyper_connect(const hyper_parameters_t* parameters,
        //                              hyper_connection_t** connection,
        //                              hyper_create_mode_t mode)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_connect(IntPtr parameters, out IntPtr connection, int mode);

        // hyper_error_t* hyper_connection_get_hyper_service_version(const hyper_connection_t* connection,
        //                                                           hyper_service_version_t* version)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_connection_get_hyper_service_version(IntPtr connection, IntPtr version);

        // hyper_error_t* hyper_query_supported_hyper_service_version_range(const hyper_parameters_t* parameters,
        //                                                                  (const hyper_service_version_t*)* version_list,
        //                                                                  size_t* version_list_size)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_query_supported_hyper_service_version_range(
            IntPtr parameters,
            out IntPtr version_list,
            out ulong version_list_size);

        // void hyper_disconnect(hyper_connection_t* connection)
        [DllImport(HyperApiDll)]
        public static extern void hyper_disconnect(IntPtr connection);

        // hyper_error_t* hyper_cancel(hyper_connection_t* connection)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_cancel(IntPtr connection);

        // hyper_connection_status_t hyper_connection_status(hyper_connection_t* connection)
        [DllImport(HyperApiDll)]
        public static extern int hyper_connection_status(IntPtr connection);

        // void hyper_connection_trace(hyper_connection_t* connection, void* debugDestination)
        [DllImport(HyperApiDll)]
        public static extern void hyper_connection_trace(IntPtr connection, IntPtr debugDestination);

        // bool hyper_connection_is_ready(hyper_connection_t* connection)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool hyper_connection_is_ready(IntPtr connection);

        // bool hyper_connection_is_capability_active(const hyper_connection_t* connection, const char* capability_flag)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool hyper_connection_is_capability_active(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string capability_flag);

        // bool hyper_connection_is_alive(hyper_connection_t* connection)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool hyper_connection_is_alive(IntPtr connection);

        // const char* hyper_connection_parameter_status(const hyper_connection_t* connection,
        //                                               const char* parameter_name)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstUtf8RetValueMarshaler))]
        public static extern string hyper_connection_parameter_status(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string parameter_name);

        // hyper_error_t* hyper_create_database(hyper_connection_t* connection, const char* path, bool failIfExists)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_create_database(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string path,
            [MarshalAs(UnmanagedType.I1)]
            bool failIfExists);

        // hyper_error_t* hyper_drop_database(hyper_connection_t* connection, const char* path, bool failIfNotExists)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_drop_database(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string path,
            [MarshalAs(UnmanagedType.I1)]
            bool failIfNotExists);

        // hyper_error_t* hyper_detach_all_databases(hyper_connection_t* connection)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_detach_all_databases(IntPtr connection);

        // hyper_error_t* hyper_detach_database(hyper_connection_t* connection, const char* alias)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_detach_database(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string alias);

        // hyper_error_t* hyper_attach_database(hyper_connection_t* connection, const char* path, const char* alias)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_attach_database(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string path,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string alias);

        // hyper_error_t* hyper_create_schema(hyper_connection_t* connection,
        //                                    const char* databaseName,
        //                                    const char* schemaName,
        //                                    bool failIfExists)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_create_schema(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string databaseName,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string schemaName,
            [MarshalAs(UnmanagedType.I1)]
            bool failIfExists);

        // bool hyper_is_valid_type_tag(hyper_type_tag_t tag)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool hyper_is_valid_type_tag(int tag);

        // hyper_table_definition_t* hyper_create_table_definition(const char* database_name,
        //                                                         const char* schema_name,
        //                                                         const char* table_name,
        //                                                         hyper_table_persistence_t persistence,
        //                                                         bool stream)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_create_table_definition(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string database_name,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string schema_name,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string table_name,
            int persistence,
            [MarshalAs(UnmanagedType.I1)]
            bool stream);

        // hyper_error_t* hyper_get_table_definition(hyper_connection_t* connection,
        //                                           const char* database_name,
        //                                           const char* schema_name,
        //                                           const char* table_name,
        //                                           hyper_table_definition_t** table_definition)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_get_table_definition(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string database_name,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string schema_name,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string table_name,
            out IntPtr table_definition);

        // void hyper_destroy_table_definition(hyper_table_definition_t* table_definition)
        [DllImport(HyperApiDll)]
        public static extern void hyper_destroy_table_definition(IntPtr table_definition);

        // hyper_error_t* hyper_copy_table_definition(const hyper_table_definition_t* table_definition,
        //                                            hyper_table_definition_t** copy)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_copy_table_definition(IntPtr table_definition, out IntPtr copy);

        // const char* hyper_table_definition_database_name(const hyper_table_definition_t* table_definition)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstUtf8RetValueMarshaler))]
        public static extern string hyper_table_definition_database_name(IntPtr table_definition);

        // const char* hyper_table_definition_schema_name(const hyper_table_definition_t* table_definition)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstUtf8RetValueMarshaler))]
        public static extern string hyper_table_definition_schema_name(IntPtr table_definition);

        // const char* hyper_table_definition_table_name(const hyper_table_definition_t* table_definition)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstUtf8RetValueMarshaler))]
        public static extern string hyper_table_definition_table_name(IntPtr table_definition);

        // hyper_table_persistence_t hyper_table_definition_table_persistence(const hyper_table_definition_t* table_definition)
        [DllImport(HyperApiDll)]
        public static extern int hyper_table_definition_table_persistence(IntPtr table_definition);

        // size_t hyper_table_definition_column_count(const hyper_table_definition_t* table_definition)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_table_definition_column_count(IntPtr table_definition);

        // hyper_type_tag_t hyper_table_definition_column_type_tag(const hyper_table_definition_t* table_definition,
        //                                                         hyper_field_index_t column_index)
        [DllImport(HyperApiDll)]
        public static extern int hyper_table_definition_column_type_tag(IntPtr table_definition, uint column_index);

        // uint32_t hyper_table_definition_column_type_oid(const hyper_table_definition_t* table_definition,
        //                                                 hyper_field_index_t column_index)
        [DllImport(HyperApiDll)]
        public static extern uint hyper_table_definition_column_type_oid(IntPtr table_definition, uint column_index);

        // hyper_type_modifier_t hyper_table_definition_column_type_modifier(const hyper_table_definition_t* table_definition,
        //                                                                   hyper_field_index_t column_index)
        [DllImport(HyperApiDll)]
        public static extern uint hyper_table_definition_column_type_modifier(IntPtr table_definition, uint column_index);

        // uint32_t hyper_get_max_length_from_modifier(hyper_type_modifier_t modifier)
        [DllImport(HyperApiDll)]
        public static extern uint hyper_get_max_length_from_modifier(uint modifier);

        // uint32_t hyper_get_precision_from_modifier(hyper_type_modifier_t modifier)
        [DllImport(HyperApiDll)]
        public static extern uint hyper_get_precision_from_modifier(uint modifier);

        // uint32_t hyper_get_scale_from_modifier(hyper_type_modifier_t modifier)
        [DllImport(HyperApiDll)]
        public static extern uint hyper_get_scale_from_modifier(uint modifier);

        // hyper_type_modifier_t hyper_encode_numeric_modifier(uint32_t precision, uint32_t scale)
        [DllImport(HyperApiDll)]
        public static extern uint hyper_encode_numeric_modifier(uint precision, uint scale);

        // hyper_type_modifier_t hyper_encode_string_modifier(uint32_t max_length)
        [DllImport(HyperApiDll)]
        public static extern uint hyper_encode_string_modifier(uint max_length);

        // hyper_field_index_t hyper_table_definition_column_index(const hyper_table_definition_t* table_definition,
        //                                                         const char* column_name)
        [DllImport(HyperApiDll)]
        public static extern uint hyper_table_definition_column_index(
            IntPtr table_definition,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string column_name);

        // const char* hyper_table_definition_column_name(const hyper_table_definition_t* table_definition,
        //                                                hyper_field_index_t column_index)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstUtf8RetValueMarshaler))]
        public static extern string hyper_table_definition_column_name(IntPtr table_definition, uint column_index);

        // bool hyper_table_definition_column_is_nullable(const hyper_table_definition_t* table_definition,
        //                                                hyper_field_index_t column_index)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool hyper_table_definition_column_is_nullable(IntPtr table_definition, uint column_index);

        // const char* hyper_table_definition_column_collation(const hyper_table_definition_t* table_definition,
        //                                                     hyper_field_index_t column_index)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstUtf8RetValueMarshaler))]
        public static extern string hyper_table_definition_column_collation(IntPtr table_definition, uint column_index);

        // hyper_error_t* hyper_table_definition_add_column(hyper_table_definition_t* table_definition,
        //                                                  const char* column_name,
        //                                                  hyper_type_tag_t type_tag,
        //                                                  hyper_type_modifier_t modifier,
        //                                                  const char* collation,
        //                                                  bool nullable)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_table_definition_add_column(
            IntPtr table_definition,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string column_name,
            int type_tag,
            uint modifier,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string collation,
            [MarshalAs(UnmanagedType.I1)]
            bool nullable);

        // hyper_error_t* hyper_create_table(hyper_connection_t* connection,
        //                                   const hyper_table_definition_t* table_definition,
        //                                   bool failIfExists)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_create_table(
            IntPtr connection,
            IntPtr table_definition,
            [MarshalAs(UnmanagedType.I1)]
            bool failIfExists);

        // void hyper_set_chunked_mode(hyper_connection_t* connection, bool chunked_mode)
        [DllImport(HyperApiDll)]
        public static extern void hyper_set_chunked_mode(IntPtr connection, [MarshalAs(UnmanagedType.I1)] bool chunked_mode);

        // size_t hyper_set_chunk_size(hyper_connection_t* connection, size_t chunk_size)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_set_chunk_size(IntPtr connection, ulong chunk_size);

        // void hyper_set_prefetch_threshold(hyper_connection_t* connection, size_t prefetch_threshold)
        [DllImport(HyperApiDll)]
        public static extern void hyper_set_prefetch_threshold(IntPtr connection, ulong prefetch_threshold);

        // hyper_error_t* hyper_execute_query(hyper_connection_t* connection,
        //                                    const char* query,
        //                                    hyper_rowset_t** rowset)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_execute_query(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string query,
            out IntPtr rowset);

        // hyper_error_t* hyper_execute_query_params(hyper_connection_t* connection,
        //                                           const char* query,
        //                                           hyper_rowset_result_format_t result_format,
        //                                           hyper_rowset_t** rowset)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_execute_query_params(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string query,
            int result_format,
            out IntPtr rowset);

        // hyper_error_t* hyper_execute_command(hyper_connection_t* connection,
        //                                      const char* query,
        //                                      int* affected_row_count)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_execute_command(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string query,
            out int affected_row_count);

        // hyper_error_t* hyper_execute_command_with_stdin_from_file(hyper_connection_t* connection,
        //                                                           const char* query,
        //                                                           const char* path)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_execute_command_with_stdin_from_file(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string query,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string path);

        // hyper_error_t* hyper_execute_command_with_stdout_to_file(hyper_connection_t* connection,
        //                                                          const char* query,
        //                                                          const char* path)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_execute_command_with_stdout_to_file(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string query,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string path);

        // hyper_error_t* hyper_set_connection_setting(hyper_connection_t* connection,
        //                                             const char* key,
        //                                             const char* value)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_set_connection_setting(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string key,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string value);

        // void hyper_string_list_destroy(hyper_string_list_t* string_list)
        [DllImport(HyperApiDll)]
        public static extern void hyper_string_list_destroy(IntPtr string_list);

        // size_t hyper_string_list_size(hyper_string_list_t* string_list)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_string_list_size(IntPtr string_list);

        // const char* hyper_string_list_at(hyper_string_list_t* string_list, int index)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstUtf8RetValueMarshaler))]
        public static extern string hyper_string_list_at(IntPtr string_list, int index);

        // hyper_error_t* hyper_get_schema_names(hyper_connection_t* connection,
        //                                       const char* database,
        //                                       hyper_string_list_t** schema_names)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_get_schema_names(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string database,
            out IntPtr schema_names);

        // hyper_error_t* hyper_get_table_names(hyper_connection_t* connection,
        //                                      const char* database,
        //                                      const char* schema,
        //                                      hyper_string_list_t** table_names)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_get_table_names(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string database,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string schema,
            out IntPtr table_names);

        // hyper_error_t* hyper_has_table(hyper_connection_t* connection,
        //                                const char* database,
        //                                const char* schema,
        //                                const char* table,
        //                                bool* exists)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_has_table(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string database,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string schema,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string table,
            [MarshalAs(UnmanagedType.I1)]
            out bool exists);

        // size_t hyper_quote_sql_identifier(char* target, size_t space, const char* value, size_t length)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_quote_sql_identifier(IntPtr target, ulong space, IntPtr value, ulong length);

        // size_t hyper_quote_sql_literal(char* target, size_t space, const char* value, size_t length)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_quote_sql_literal(IntPtr target, ulong space, IntPtr value, ulong length);

        // hyper_error_t* hyper_prepare(hyper_connection_t* connection, const char* statement_name, const char* query)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_prepare(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string statement_name,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string query);

        // hyper_error_t* hyper_execute_prepared(hyper_connection_t* connection,
        //                                       const char* statement_name,
        //                                       hyper_rowset_result_format_t result_format,
        //                                       hyper_rowset_t** rowset)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_execute_prepared(
            IntPtr connection,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string statement_name,
            int result_format,
            out IntPtr rowset);

        // void hyper_close_rowset(hyper_rowset_t* rowset)
        [DllImport(HyperApiDll)]
        public static extern void hyper_close_rowset(IntPtr rowset);

        // hyper_rowset_type_t hyper_rowset_get_type(hyper_rowset_t* rowset)
        [DllImport(HyperApiDll)]
        public static extern int hyper_rowset_get_type(IntPtr rowset);

        // const hyper_table_definition_t* hyper_rowset_get_table_definition(const hyper_rowset_t* rowset)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_rowset_get_table_definition(IntPtr rowset);

        // int64_t hyper_rowset_get_affected_row_count(const hyper_rowset_t* rowset)
        [DllImport(HyperApiDll)]
        public static extern long hyper_rowset_get_affected_row_count(IntPtr rowset);

        // hyper_error_t* hyper_rowset_get_next_chunk(hyper_rowset_t* rowset, hyper_rowset_chunk_t** rowset_chunk)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_rowset_get_next_chunk(IntPtr rowset, out IntPtr rowset_chunk);

        // size_t hyper_rowset_chunk_row_count(const hyper_rowset_chunk_t* rowset_chunk)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_rowset_chunk_row_count(IntPtr rowset_chunk);

        // hyper_value_t hyper_rowset_chunk_field_value(const hyper_rowset_chunk_t* rowset_chunk,
        //                                              hyper_row_index_t row_index,
        //                                              hyper_field_index_t field_index)
        [DllImport(HyperApiDll)]
        public static extern hyper_value_t hyper_rowset_chunk_field_value(
            IntPtr rowset_chunk,
            uint row_index,
            uint field_index);

        // bool hyper_rowset_chunk_field_is_null(const hyper_rowset_chunk_t* rowset_chunk,
        //                                       hyper_field_index_t row_index,
        //                                       hyper_field_index_t field_index)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool hyper_rowset_chunk_field_is_null(IntPtr rowset_chunk, uint row_index, uint field_index);

        // void hyper_rowset_chunk_field_values(hyper_rowset_chunk_t* rowset_chunk,
        //                                      size_t* col_count,
        //                                      size_t* row_count,
        //                                      (const (const uint8_t*)*)* values,
        //                                      (const size_t*)* sizes)
        [DllImport(HyperApiDll)]
        public static extern void hyper_rowset_chunk_field_values(
            IntPtr rowset_chunk,
            out ulong col_count,
            out ulong row_count,
            out IntPtr values,
            out IntPtr sizes);

        // bool hyper_rowset_has_copy_data(const hyper_rowset_t* rowset)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool hyper_rowset_has_copy_data(IntPtr rowset);

        // hyper_error_t* hyper_rowset_get_copy_data(hyper_rowset_t* rowset, char** buffer, size_t* length)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_rowset_get_copy_data(IntPtr rowset, out IntPtr buffer, out ulong length);

        // void hyper_rowset_free_copy_data(char* buffer)
        [DllImport(HyperApiDll)]
        public static extern void hyper_rowset_free_copy_data(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string buffer);

        // void hyper_destroy_rowset_chunk(const hyper_rowset_chunk_t* rowset_chunk)
        [DllImport(HyperApiDll)]
        public static extern void hyper_destroy_rowset_chunk(IntPtr rowset_chunk);

        // hyper_data_chunk_t* hyper_create_data_chunk()
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_create_data_chunk();

        // hyper_error_t* hyper_resize_data_chunk(hyper_data_chunk_t* data_chunk, size_t size)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_resize_data_chunk(IntPtr data_chunk, ulong size);

        // uint8_t* hyper_get_chunk_data(const hyper_data_chunk_t* data_chunk)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_get_chunk_data(IntPtr data_chunk);

        // size_t hyper_get_chunk_header_size(const hyper_data_chunk_t* data_chunk)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_get_chunk_header_size(IntPtr data_chunk);

        // size_t hyper_get_chunk_data_size(const hyper_data_chunk_t* data_chunk)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_get_chunk_data_size(IntPtr data_chunk);

        // void hyper_destroy_data_chunk(hyper_data_chunk_t* data_chunk)
        [DllImport(HyperApiDll)]
        public static extern void hyper_destroy_data_chunk(IntPtr data_chunk);

        // hyper_error_t* hyper_create_inserter(hyper_connection_t* connection,
        //                                      const hyper_table_definition_t* table_definition,
        //                                      hyper_inserter_t** inserter)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_create_inserter(IntPtr connection, IntPtr table_definition, out IntPtr inserter);

        // hyper_error_t* hyper_init_bulk_insert(hyper_inserter_t* inserter,
        //                                       const hyper_table_definition_t* table_definition,
        //                                       const char* select_list)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_init_bulk_insert(
            IntPtr inserter,
            IntPtr table_definition,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string select_list);

        // hyper_error_t* hyper_insert_computed_expressions(hyper_inserter_t* inserter, const char* select_list)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_insert_computed_expressions(
            IntPtr inserter,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string select_list);

        // hyper_error_t* hyper_inserter_insert_chunk(hyper_inserter_t* inserter,
        //                                            const uint8_t* data_chunk,
        //                                            size_t bytes)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_insert_chunk(IntPtr inserter, IntPtr data_chunk, ulong bytes);

        // hyper_error_t* hyper_close_inserter(hyper_inserter_t* inserter, bool insert_data)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_close_inserter(IntPtr inserter, [MarshalAs(UnmanagedType.I1)] bool insert_data);

        // size_t hyper_write_null(uint8_t* target, size_t space)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_null(IntPtr target, ulong space);

        // size_t hyper_write_header(uint8_t* target, size_t space)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_header(IntPtr target, ulong space);

        // size_t hyper_write_int8(uint8_t* target, size_t space, int8_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_int8(IntPtr target, ulong space, sbyte value);

        // size_t hyper_write_int8_not_null(uint8_t* target, size_t space, int8_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_int8_not_null(IntPtr target, ulong space, sbyte value);

        // size_t hyper_write_int16(uint8_t* target, size_t space, int16_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_int16(IntPtr target, ulong space, short value);

        // size_t hyper_write_int16_not_null(uint8_t* target, size_t space, int16_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_int16_not_null(IntPtr target, ulong space, short value);

        // size_t hyper_write_int32(uint8_t* target, size_t space, int32_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_int32(IntPtr target, ulong space, int value);

        // size_t hyper_write_int32_not_null(uint8_t* target, size_t space, int32_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_int32_not_null(IntPtr target, ulong space, int value);

        // size_t hyper_write_int64(uint8_t* target, size_t space, int64_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_int64(IntPtr target, ulong space, long value);

        // size_t hyper_write_int64_not_null(uint8_t* target, size_t space, int64_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_int64_not_null(IntPtr target, ulong space, long value);

        // size_t hyper_write_data128(uint8_t* target, size_t space, hyper_data128_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_data128(IntPtr target, ulong space, hyper_data128_t value);

        // size_t hyper_write_data128_not_null(uint8_t* target, size_t space, hyper_data128_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_data128_not_null(IntPtr target, ulong space, hyper_data128_t value);

        // size_t hyper_write_varbinary(uint8_t* target, size_t space, const uint8_t* value, size_t length)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_varbinary(IntPtr target, ulong space, IntPtr value, ulong length);

        // size_t hyper_write_varbinary_not_null(uint8_t* target, size_t space, const uint8_t* value, size_t length)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_write_varbinary_not_null(IntPtr target, ulong space, IntPtr value, ulong length);

        // int8_t hyper_read_int8(const uint8_t* source)
        [DllImport(HyperApiDll)]
        public static extern sbyte hyper_read_int8(IntPtr source);

        // int16_t hyper_read_int16(const uint8_t* source)
        [DllImport(HyperApiDll)]
        public static extern short hyper_read_int16(IntPtr source);

        // int32_t hyper_read_int32(const uint8_t* source)
        [DllImport(HyperApiDll)]
        public static extern int hyper_read_int32(IntPtr source);

        // int64_t hyper_read_int64(const uint8_t* source)
        [DllImport(HyperApiDll)]
        public static extern long hyper_read_int64(IntPtr source);

        // hyper_data128_t hyper_read_data128(const uint8_t* source)
        [DllImport(HyperApiDll)]
        public static extern hyper_data128_t hyper_read_data128(IntPtr source);

        // const uint8_t* hyper_read_varbinary(const uint8_t* source)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_read_varbinary(IntPtr source);

        // hyper_error_t* hyper_create_inserter_buffer(hyper_inserter_t* inserter,
        //                                             const hyper_table_definition_t* table_definition,
        //                                             const char* select_list,
        //                                             hyper_inserter_buffer_t** buffer)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_create_inserter_buffer(
            IntPtr inserter,
            IntPtr table_definition,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string select_list,
            out IntPtr buffer);

        // hyper_error_t* hyper_inserter_buffer_flush(hyper_inserter_buffer_t* buffer)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_buffer_flush(IntPtr buffer);

        // void hyper_inserter_buffer_destroy(hyper_inserter_buffer_t* buffer)
        [DllImport(HyperApiDll)]
        public static extern void hyper_inserter_buffer_destroy(IntPtr buffer);

        // hyper_error_t* hyper_inserter_buffer_add_null(hyper_inserter_buffer_t* buffer)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_buffer_add_null(IntPtr buffer);

        // hyper_error_t* hyper_inserter_buffer_add_bool(hyper_inserter_buffer_t* buffer, bool value)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_buffer_add_bool(IntPtr buffer, [MarshalAs(UnmanagedType.I1)] bool value);

        // hyper_error_t* hyper_inserter_buffer_add_int16(hyper_inserter_buffer_t* buffer, int16_t value)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_buffer_add_int16(IntPtr buffer, short value);

        // hyper_error_t* hyper_inserter_buffer_add_int32(hyper_inserter_buffer_t* buffer, int32_t value)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_buffer_add_int32(IntPtr buffer, int value);

        // hyper_error_t* hyper_inserter_buffer_add_int64(hyper_inserter_buffer_t* buffer, int64_t value)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_buffer_add_int64(IntPtr buffer, long value);

        // hyper_error_t* hyper_inserter_buffer_add_double(hyper_inserter_buffer_t* buffer, double value)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_buffer_add_double(IntPtr buffer, double value);

        // hyper_error_t* hyper_inserter_buffer_add_binary(hyper_inserter_buffer_t* buffer,
        //                                                 const uint8_t* value,
        //                                                 size_t size)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_buffer_add_binary(IntPtr buffer, IntPtr value, ulong size);

        // hyper_error_t* hyper_inserter_buffer_add_date(hyper_inserter_buffer_t* buffer,
        //                                               int32_t year,
        //                                               int16_t month,
        //                                               int16_t day)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_buffer_add_date(IntPtr buffer, int year, short month, short day);

        // hyper_error_t* hyper_inserter_buffer_add_raw(hyper_inserter_buffer_t* buffer,
        //                                              const uint8_t* value,
        //                                              size_t size)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_inserter_buffer_add_raw(IntPtr buffer, IntPtr value, ulong size);

        // hyper_date_components_t hyper_decode_date(hyper_date_t date)
        [DllImport(HyperApiDll)]
        public static extern hyper_date_components_t hyper_decode_date(uint date);

        // hyper_date_t hyper_encode_date(hyper_date_components_t components)
        [DllImport(HyperApiDll)]
        public static extern uint hyper_encode_date(hyper_date_components_t components);

        // size_t hyper_date_to_string(char* target, size_t space, hyper_date_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_date_to_string(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string target,
            ulong space,
            uint value);

        // hyper_time_components_t hyper_decode_time(hyper_time_t time)
        [DllImport(HyperApiDll)]
        public static extern hyper_time_components_t hyper_decode_time(ulong time);

        // hyper_time_t hyper_encode_time(hyper_time_components_t components)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_encode_time(hyper_time_components_t components);

        // size_t hyper_time_to_string(char* target, size_t space, hyper_time_t value)
        [DllImport(HyperApiDll)]
        public static extern ulong hyper_time_to_string(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string target,
            ulong space,
            ulong value);

        // hyper_interval_components_t hyper_decode_interval(hyper_interval_t interval)
        [DllImport(HyperApiDll)]
        public static extern hyper_interval_components_t hyper_decode_interval(hyper_data128_t interval);

        // hyper_interval_t hyper_encode_interval(hyper_interval_components_t components)
        [DllImport(HyperApiDll)]
        public static extern hyper_data128_t hyper_encode_interval(hyper_interval_components_t components);

        // hyper_error_t* hyper_numeric_from_string((const char*)* iter,
        //                                          const char* limit,
        //                                          uint32_t precision,
        //                                          uint32_t scale,
        //                                          int64_t* result)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_numeric_from_string(
            out IntPtr iter,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string limit,
            uint precision,
            uint scale,
            IntPtr result);

        // hyper_error_t* hyper_big_numeric_from_string((const char*)* iter,
        //                                              const char* limit,
        //                                              uint32_t precision,
        //                                              uint32_t scale,
        //                                              hyper_data128_t* result)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_big_numeric_from_string(
            out IntPtr iter,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string limit,
            uint precision,
            uint scale,
            IntPtr result);

        // char* hyper_numeric_to_string(char* target, size_t space, int64_t value, uint32_t scale)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstUtf8RetValueMarshaler))]
        public static extern string hyper_numeric_to_string(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string target,
            ulong space,
            long value,
            uint scale);

        // char* hyper_big_numeric_to_string(char* target, size_t space, hyper_data128_t value, uint32_t scale)
        [DllImport(HyperApiDll)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstUtf8RetValueMarshaler))]
        public static extern string hyper_big_numeric_to_string(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string target,
            ulong space,
            hyper_data128_t value,
            uint scale);

        // hyper_error_t* hyper_numeric_change_scale(int64_t value, uint32_t scale, uint32_t new_scale, int64_t* result)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_numeric_change_scale(long value, uint scale, uint new_scale, IntPtr result);

        // hyper_error_t* hyper_big_numeric_change_scale(hyper_data128_t value,
        //                                               uint32_t scale,
        //                                               uint32_t new_scale,
        //                                               hyper_data128_t* result)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_big_numeric_change_scale(
            hyper_data128_t value,
            uint scale,
            uint new_scale,
            IntPtr result);

        // int64_t hyper_numeric_round_to_int64(int64_t value, uint32_t scale)
        [DllImport(HyperApiDll)]
        public static extern long hyper_numeric_round_to_int64(long value, uint scale);

        // hyper_error_t* hyper_big_numeric_round_to_int64(hyper_data128_t value, uint32_t scale, int64_t* result)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_big_numeric_round_to_int64(hyper_data128_t value, uint scale, IntPtr result);

        // hyper_error_t* hyper_numeric_from_double(double value, uint32_t scale, int64_t* result)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_numeric_from_double(double value, uint scale, IntPtr result);

        // hyper_error_t* hyper_big_numeric_from_double(double value, uint32_t scale, hyper_data128_t* result)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_big_numeric_from_double(double value, uint scale, IntPtr result);

        // double hyper_numeric_to_double(int64_t value, uint32_t scale)
        [DllImport(HyperApiDll)]
        public static extern double hyper_numeric_to_double(long value, uint scale);

        // double hyper_big_numeric_to_double(hyper_data128_t value, uint32_t scale)
        [DllImport(HyperApiDll)]
        public static extern double hyper_big_numeric_to_double(hyper_data128_t value, uint scale);

        // hyper_error_t* hyper_copy_data(hyper_connection_t* connection, const uint8_t* buffer, size_t size)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_copy_data(IntPtr connection, IntPtr buffer, ulong size);

        // hyper_error_t* hyper_copy_end(hyper_connection_t* connection)
        [DllImport(HyperApiDll)]
        public static extern IntPtr hyper_copy_end(IntPtr connection);

        // hyper_log_function_t hyper_log_set_log_function(hyper_log_function_t log_function, void* context)
        [DllImport(HyperApiDll)]
        public static extern hyper_log_function_t hyper_log_set_log_function(
            hyper_log_function_t log_function,
            IntPtr context);

        // hyper_log_level_t hyper_log_set_log_level(hyper_log_level_t log_level)
        [DllImport(HyperApiDll)]
        public static extern int hyper_log_set_log_level(int log_level);

        // void hyper_default_log_function(hyper_log_level_t log_level,
        //                                 const char* topic,
        //                                 const char* json_value,
        //                                 void* context)
        [DllImport(HyperApiDll)]
        public static extern void hyper_default_log_function(
            int log_level,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string topic,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string json_value,
            IntPtr context);

        // void hyper_log_event(hyper_log_level_t log_level, const char* topic, const char* json_value)
        [DllImport(HyperApiDll)]
        public static extern void hyper_log_event(
            int log_level,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string topic,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8ParamMarshaler))]
            string json_value);
    }
}
