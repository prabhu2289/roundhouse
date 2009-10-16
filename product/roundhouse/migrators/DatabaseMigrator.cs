using roundhouse.sql;

namespace roundhouse.migrators
{
    public interface DatabaseMigrator
    {
        Database database { get; set; }
        void create_or_restore_database();
        void restore_database(string restore_from_path);
        void delete_database();
        void verify_or_create_roundhouse_tables();
        long version_the_database(string repository_path, string repository_version);
        void run_sql(string sql_to_run, string script_name, bool run_this_script_once, long version_id);
    }
}