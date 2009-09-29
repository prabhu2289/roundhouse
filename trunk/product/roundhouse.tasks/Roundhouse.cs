﻿namespace roundhouse.tasks
{
    using System;
    using System.Collections.Generic;
    using Castle.Windsor;
    using infrastructure;
    using infrastructure.containers;
    using infrastructure.filesystem;
    using infrastructure.logging;
    using infrastructure.logging.custom;
    using log4net;
    using Microsoft.Build.Framework;
    using NAnt.Core;
    using NAnt.Core.Attributes;
    using runners;
    using sql;

    [TaskName("roundhouse")]
    public class Roundhouse : Task, ITask
    {
        private const string UP_FOLDER_NAME = "up";
        private const string DOWN_FOLDER_NAME = "down";
        private const string RUN_FIRST_FOLDER_NAME = "runFirst";
        private const string FUNCTIONS_FOLDER_NAME = "functions";
        private const string VIEWS_FOLDER_NAME = "views";
        private const string SPROCS_FOLDER_NAME = "sprocs";
        private const string PERMISSIONS_FOLDER_NAME = "permissions";
        private const string VERSION_TABLE_NAME = "dbo._version";
        private readonly ILog the_logger = LogManager.GetLogger(typeof(Roundhouse));

        #region MSBuild

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        /// <summary>
        /// The function for the MSBuild task that actually does the task
        /// </summary>
        /// <returns>true if the task is successful</returns>
        bool ITask.Execute()
        {
            run_the_task();
            return true;
        }

        #endregion

        #region NAnt

        /// <summary>
        /// Executes the NAnt task
        /// </summary>
        protected override void ExecuteTask()
        {
            run_the_task();
        }

        #endregion

        #region Properties

        [Required]
        [TaskAttribute("serverName", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ServerName { get; set; }

        [Required]
        [TaskAttribute("databaseName", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string DatabaseName { get; set; }

        [Required]
        [TaskAttribute("sqlFilesDirectory", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string SqlFilesDirectory { get; set; }

        [TaskAttribute("repositoryUrl", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string RepositoryUrl { get; set; }

        [TaskAttribute("upFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string UpFolderName { get; set; }

        [TaskAttribute("downFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string DownFolderName { get; set; }

        [TaskAttribute("runFirstFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string RunFirstFolderName { get; set; }

        [TaskAttribute("functionsFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string FunctionsFolderName { get; set; }

        [TaskAttribute("viewsFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string ViewsFolderName { get; set; }

        [TaskAttribute("sprocsFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string SprocsFolderName { get; set; }

        [TaskAttribute("permissionsFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string PermissionsFolderName { get; set; }

        [TaskAttribute("versionTableName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string VersionTableName { get; set; }

        #endregion

        public void run_the_task()
        {
            Container.initialize_with(build_the_container());
            set_up_properties();

            infrastructure.logging.Log.bound_to(this).log_an_info_event_containing(
                "Executing {0} against contents of {1}.",
                ApplicationParameters.name,
                SqlFilesDirectory);

            
            IRunner roundhouse_runner = new RoundhouseRunner(
                                                ServerName, 
                                                DatabaseName, 
                                                RepositoryUrl, 
                                                SqlFilesDirectory, 
                                                UpFolderName, 
                                                DownFolderName,
                                                RunFirstFolderName, 
                                                FunctionsFolderName, 
                                                ViewsFolderName, 
                                                SprocsFolderName, 
                                                PermissionsFolderName,
                                                VersionTableName,
                                                Container.get_an_instance_of<FileSystemAccess>(),
                                                Container.get_an_instance_of<Database>()
                                                );
            try
            {
                roundhouse_runner.run();
            }
            catch (Exception exception)
            {
                infrastructure.logging.Log.bound_to(this).log_an_error_event_containing("{0} encountered an error:{1}{2}", ApplicationParameters.name,
                                                                                        Environment.NewLine, exception);
                throw;
            }
        }

        private InversionContainer build_the_container()
        {
            IWindsorContainer windsor_container = new WindsorContainer();

            Logger nant_logger = new NAntLogger(this);
            Logger msbuild_logger = new MSBuildLogger(this, BuildEngine);
            Logger log4net_logger = new Log4NetLogger(the_logger);
            Logger multi_logger = new MultipleLogger(new List<Logger> { nant_logger, msbuild_logger, log4net_logger });

            windsor_container.Kernel.AddComponentInstance<Logger>(multi_logger);

            windsor_container.AddComponent<FileSystemAccess, WindowsFileSystemAccess>();
            windsor_container.AddComponent<Database, SqlServerDatabase>();
            windsor_container.AddComponent<LogFactory, MultipleLoggerLogFactory>();

            return new infrastructure.containers.custom.WindsorContainer(windsor_container);
        }

        public void set_up_properties()
        {
            if (string.IsNullOrEmpty(UpFolderName))
            {
                UpFolderName = UP_FOLDER_NAME;
            }
            if (string.IsNullOrEmpty(DownFolderName))
            {
                DownFolderName = DOWN_FOLDER_NAME;
            }
            if (string.IsNullOrEmpty(RunFirstFolderName))
            {
                RunFirstFolderName = RUN_FIRST_FOLDER_NAME;
            }
            if (string.IsNullOrEmpty(FunctionsFolderName))
            {
                FunctionsFolderName = FUNCTIONS_FOLDER_NAME;
            }
            if (string.IsNullOrEmpty(ViewsFolderName))
            {
                ViewsFolderName = VIEWS_FOLDER_NAME;
            }
            if (string.IsNullOrEmpty(SprocsFolderName))
            {
                SprocsFolderName = SPROCS_FOLDER_NAME;
            }
            if (string.IsNullOrEmpty(PermissionsFolderName))
            {
                PermissionsFolderName = PERMISSIONS_FOLDER_NAME;
            }
            if (string.IsNullOrEmpty(VersionTableName))
            {
                VersionTableName = VERSION_TABLE_NAME;
            }
        }
    }
}