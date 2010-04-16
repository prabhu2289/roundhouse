using System.Collections.Generic;
using System.Data;
using roundhouse.connections;
using roundhouse.parameters;
using roundhouse.sql;

namespace roundhouse.databases
{
    public abstract class DefaultDatabase<DBCONNECTION,DBPARAMETER> : Database
    {
        public string server_name { get; set; }
        public string database_name { get; set; }
        public string provider { get; set; }
        public string connection_string { get; set; }
        public string roundhouse_schema_name { get; set; }
        public string version_table_name { get; set; }
        public string scripts_run_table_name { get; set; }
        public string scripts_run_errors_table_name { get; set; }
        public string user_name { get; set; }
        public string master_database_name { get; set; }

        public string sql_statement_separator_regex_pattern
        {
            get { return sql_scripts.separator_characters_regex; }
        }

        public string custom_create_database_script { get; set; }
        public int command_timeout { get; set; }
        public int restore_timeout { get; set; }
        protected bool split_batches = false;
        public virtual bool split_batch_statements
        {
            get { return split_batches; }
            set { split_batches = value; }
        }

        protected IConnection<DBCONNECTION> server_connection;
        
        private bool disposing;
        protected SqlScript sql_scripts;

        //this method must set the provider
        public abstract void initialize_connection();
        public abstract void open_connection(bool with_transaction);
        public abstract void close_connection();
        public abstract void rollback();

        public virtual void create_database_if_it_doesnt_exist()
        {
            use_database(master_database_name);
            string create_script = sql_scripts.create_database(database_name);
            if (!string.IsNullOrEmpty(custom_create_database_script))
            {
                create_script = custom_create_database_script;
            }
            run_sql(create_script);
        }

        public void set_recovery_mode(bool simple)
        {
            use_database(master_database_name);
            run_sql(sql_scripts.set_recovery_mode(database_name, simple));
        }

        public void backup_database(string output_path_minus_database)
        {
            //todo: backup database is not a script - it is a command
            //Server sql_server =
            //    new Server(new ServerConnection(new SqlConnection(build_connection_string(server_name, database_name))));
            //sql_server.BackupDevices.Add(new BackupDevice(sql_server,database_name));
        }

        public void restore_database(string restore_from_path, string custom_restore_options)
        {
            use_database(master_database_name);

            int current_connetion_timeout = command_timeout;
            command_timeout = restore_timeout;
            run_sql(sql_scripts.restore_database(database_name, restore_from_path, custom_restore_options));
            command_timeout = current_connetion_timeout;
        }

        public virtual void delete_database_if_it_exists()
        {
            use_database(master_database_name);
            run_sql(sql_scripts.delete_database(database_name));
        }

        public void use_database(string database_name)
        {
            run_sql(sql_scripts.use_database(database_name));
        }

        public void create_roundhouse_schema_if_it_doesnt_exist()
        {
            run_sql(sql_scripts.create_roundhouse_schema(roundhouse_schema_name));
        }

        public void create_roundhouse_version_table_if_it_doesnt_exist()
        {
            run_sql(sql_scripts.create_roundhouse_version_table(roundhouse_schema_name, version_table_name));
        }

        public void create_roundhouse_scripts_run_table_if_it_doesnt_exist()
        {
            run_sql(sql_scripts.create_roundhouse_scripts_run_table(roundhouse_schema_name, version_table_name, scripts_run_table_name));
        }

        public void create_roundhouse_scripts_run_errors_table_if_it_doesnt_exist()
        {
            run_sql(sql_scripts.create_roundhouse_scripts_run_errors_table(roundhouse_schema_name, scripts_run_errors_table_name));
        }

        public virtual void run_sql(string sql_to_run)
        {
            run_sql(sql_to_run, null);
        }

        protected abstract void run_sql(string sql_to_run, IList<IParameter<DBPARAMETER>> parameters);
        
        public virtual void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id)
        {
            var parameters = new List<IParameter<DBPARAMETER>>
                                 {
                                     create_parameter("version_id", DbType.Int64, version_id, null), 
                                     create_parameter("script_name", DbType.AnsiString, script_name, 255), 
                                     create_parameter("sql_to_run", DbType.AnsiString, sql_to_run, null), 
                                     create_parameter("sql_to_run_hash", DbType.AnsiString, sql_to_run_hash, 512), 
                                     create_parameter("run_this_script_once", DbType.Boolean, run_this_script_once, null), 
                                     create_parameter("user_name", DbType.AnsiString, user_name, 50)
                                 };
            run_sql(sql_scripts.insert_script_run_parameterized(roundhouse_schema_name, scripts_run_table_name), parameters);
        }

        public void insert_script_run_error(string script_name, string sql_to_run, string sql_erroneous_part, string error_message, long version_id)
        {
            var parameters = new List<IParameter<DBPARAMETER>>
                                 {
                                     create_parameter("version_id", DbType.Int64, version_id, null), 
                                     create_parameter("script_name", DbType.AnsiString, script_name, 255), 
                                     create_parameter("sql_to_run", DbType.AnsiString, sql_to_run, null), 
                                     create_parameter("sql_erroneous_part", DbType.AnsiString, sql_erroneous_part, null), 
                                     create_parameter("error_message", DbType.AnsiString, error_message, 255), 
                                     create_parameter("user_name", DbType.AnsiString, user_name, 50)
                                 };
            run_sql(sql_scripts.insert_script_run_error_parameterized(roundhouse_schema_name, scripts_run_errors_table_name), parameters);
        }

        public string get_version(string repository_path)
        {
            var parameters = new List<IParameter<DBPARAMETER>> { create_parameter("repository_path", DbType.AnsiString, repository_path, 255) };
            return (string)run_sql_scalar(sql_scripts.get_version_parameterized(roundhouse_schema_name, version_table_name), parameters);
        }

        public virtual long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            var insert_parameters = new List<IParameter<DBPARAMETER>>
                                 {
                                     create_parameter("repository_path", DbType.AnsiString, repository_path, 255), 
                                     create_parameter("repository_version", DbType.AnsiString, repository_version, 35), 
                                     create_parameter("user_name", DbType.AnsiString, user_name, 50)
                                 };
            run_sql(sql_scripts.insert_version_parameterized(roundhouse_schema_name, version_table_name), insert_parameters);

            var select_parameters = new List<IParameter<DBPARAMETER>> { create_parameter("repository_path", DbType.AnsiString, repository_path, 255) };
            return (long)run_sql_scalar(sql_scripts.get_version_id_parameterized(roundhouse_schema_name, version_table_name), select_parameters);
        }

        public string get_current_script_hash(string script_name)
        {
            var parameters = new List<IParameter<DBPARAMETER>> { create_parameter("script_name", DbType.AnsiString, script_name, 255) };
            return (string)run_sql_scalar(sql_scripts.get_current_script_hash_parameterized(roundhouse_schema_name, scripts_run_table_name), parameters);
        }

        public bool has_run_script_already(string script_name)
        {
            bool script_has_run = false;

            IList<IParameter<DBPARAMETER>> parameters = new List<IParameter<DBPARAMETER>>
                                                     {
                                                         create_parameter("script_name", DbType.AnsiString, script_name, 255)
                                                     };

            DataTable data_table = execute_datatable(sql_scripts.has_script_run_parameterized(roundhouse_schema_name, scripts_run_table_name), parameters);
            if (data_table.Rows.Count > 0)
            {
                script_has_run = true;
            }

            return script_has_run;
        }

        public virtual object run_sql_scalar(string sql_to_run)
        {
            return run_sql_scalar(sql_to_run, null);
        }

        protected abstract object run_sql_scalar(string sql_to_run, IList<IParameter<DBPARAMETER>> parameters);

        protected abstract DataTable execute_datatable(string sql_to_run, IEnumerable<IParameter<DBPARAMETER>> parameters);

        protected abstract IParameter<DBPARAMETER> create_parameter(string name, DbType type, object value, int? size);
       

        public void Dispose()
        {
            if (!disposing)
            {
                server_connection.Dispose();
                disposing = true;
            }
        } 
    }
}