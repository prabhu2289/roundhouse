     //                          :iji                         
     //                         #WWWWWW                       
     //                     :,#WWWWWWWGW                      
     //                   ,WWWWWWWWWWWWW;K                    
     //                  iWWWWWWWWWWWWWWW D                   
     //                  WWWWWWWWWWWWWWWWWW:                  
     //                 WWWWWWWWWWWWWWWWWWWW                  
     //                ,WWWWWWWWWWWWWWWWWWWW.                 
     //                iWWWWWWWWWWWWWWWWWWWWi                 
     //                 WWWWWWWWWWWWWWWWWWWWK                 
     //                ,WWWWWWWWWWtWWWWWWWWW#                 
     //                LWWWWWWWWWL: LW#WWWWWW:                
     //                ,WWWWWWWW#i    ,WWWWWWj                
     //                ,WWWWGWWW      .WWWWWWW                
     //                 WWWWWWWK       EWWWWW#                
     //                 #WWW:::         WWWWW#                
     //                 tWW.           .WWW#W#                
     //                  DW j :EKGf     WWD.WW                
     //  .G.             #WWWE WWW#W    .fK.W#                
     //   #j             LWWW# L ,      Li  WD                
     //   .EL            ;WEWG          W  :W                 
     //    E;:           i  W,         :W  KW                 
     //     #j              W.         iE tWE                 
     //     fKWD          D W          K# LW                  
     //     :WWWD         WGW   ,G     i.                     
     //     :WWW#,:#iK    EWW:L:      W#:  G                  
     //      fWW K  # ,   :WWWW;.  t #WL,  WL                 
     //       WW#f   :tjDWWWWWWWWWWWWWWW   EW                 
     //      .WWWGL  WWWWWWWWWt:  GWWEW    WW:                
     //       WWWW,. #WWWWWWWWE,,iEWiW#   GWWj                
     //       #WWWWD  WWWWWWWWWWWWWWWWL  fWWWW;               
     //       :WWWWfD; WWWWWWWWWWWWWWW. ;WWWWWWL              
     //        #WWWW.W: WWWWWWWWWWWWWWGWWWWWWWWWW             
     //        iWWWWWK, #WWWWWWWWWWWWWWWWWWWWWWWW#            
     //         WWWWW:D :WWWWWWWWWWWWWWWWWWWWWWWWWW.          
     //         :WWWW# G  #WWWWWWWWWWWWWWWWWWWWWWWWW,         
     //          WWWWWW,   WWWWWWWWWWWWWWWWWWWWWWWWWW         
     //         ;WWWWWW.K  KWWWWWWWWWWWWWWWWWWWWWWWWWD        
     //         DWWWWWWW L  WWWWWWWWWWWWWWWWWWWWWWWWWWf       
     //         WWWWWWW L:  :WWWWWWWWWWWWWWWWWWWWWWWWWWK      
     //         #WWWWWE: jW  KWWWWWWWWWWWWWWWWWWWWWWWWWW;     
     //         WWWWWWWGt; ;  WWWWWWWWWWWWWWWWWWWWWWWWWWW     
     //        GWWWWWWWW; GjEjtWWWWWWWWWWWWWWWWWWWWWWEWWW.    
     //        WWWWWWtKtWt WWW EWWWWWWWWWWWWWWWWWWj    #WD    
     //;.     WWWWWWWi  DKGGWWK WWWWWWWWWWWWWWWWL       WD    
     //;:    .WWWWWWWW   W  WWWijWWWWWWWWWWWWWWf        #E    
     //t :   WWWWWL EW:  :WG,WWK WWWWWWWWWWWWW,         #E    
     //L i  DWWWWiWE .. : fDGD:j  WWWWWWWWWWW           LE    
     //G .K WWWWWfLWi    K W .f.  jWWWWWWWWW.           tL    
     //G   #K##fWWW#G     :DLG. W  WWWWWWWWW            t     
     //fjj;   t LWE       tKWG   : WWWWWWWW:            L     
     //Df   j  :WWK        WWW:.Dt LWWWWWWWj            W     
     //WG  W   WWG         WWWWf  E WL .WWW             G     
     //WWKK    :WG         WWWWjiG .Li  :Wi             .     
     //#WW     .WW         WWWWW  G;   : K                    
     //WWWL     WWj       :WWWWW#D      ,              L      
     //#WWW   L GWW        KWWWWWL     .#              K      
     //#WWK  G  W:WW        WW#WKE     LL              f      
     //jWWW,Ef     Lf                                  .      
     // t##K#       i,                                j       
     //      ;i                                           

namespace roundhouse.databases
{
    using System;

    public interface Database : IDisposable
    {
        string server_name { get; set; }
        string database_name { get; set; }
        string provider { get; set; }
        string connection_string { get; set; }
        string roundhouse_schema_name { get; set; }
        string version_table_name { get; set; }
        string scripts_run_table_name { get; set; }
        string user_name { get; set; }
        string sql_statement_separator_regex_pattern { get;}
        string custom_create_database_script { get; set; }
        int command_timeout { get; set; }
        int restore_timeout { get; set; }
        bool split_batch_statements { get; set; }
        
        void initialize_connection();
        void open_connection(bool with_transaction);
        void close_connection();

        void create_database_if_it_doesnt_exist();
        void set_recovery_mode(bool simple);
        void backup_database(string output_path_minus_database);
        void restore_database(string restore_from_path, string custom_restore_options);
        void delete_database_if_it_exists();
        void use_database(string database_name);
        void create_roundhouse_schema_if_it_doesnt_exist();
        void create_roundhouse_version_table_if_it_doesnt_exist();
        void create_roundhouse_scripts_run_table_if_it_doesnt_exist();
        void run_sql(string sql_to_run);
        void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id);
        
        string get_version(string repository_path);
        long insert_version_and_get_version_id(string repository_path, string repository_version);
        bool has_run_script_already(string script_name);
        string get_current_script_hash(string script_name);
        object run_sql_scalar(string sql_to_run);
    }
}