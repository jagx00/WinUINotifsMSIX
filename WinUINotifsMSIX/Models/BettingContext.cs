//using Microsoft.AppCenter.Ingestion.Models;
using JAGLog.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Notifications;
using WinUINotifsMSIX;
//using Log = Serilog.Log;
using WinUINotifsMSIX.ViewModels;


// 20211024

// NuGet will still show all the 3 EntityFrameworkCore 3.1.9 modules as requiring an update, but DO NOT try to update as after at least two years it won't work.
// It looks like UWP is dead: see below.

// See https://stackoverflow.com/questions/66242367/uwp-with-entity-framework-core-5-0 for original issue.
// EF Core 5.0 is planned to run on any .NET Standard 2.1 platform, including .NET 5.0. 
// you can't use EF Core 5.0 in UWP platform.
//
// UWP support for .Net Standard 2.1 is still To Be Determined (TBD).

// https://docs.microsoft.com/en-us/answers/questions/40535/please-tell-me-when-will-uwp-support-net-standard.html
// Maybe Win 11 or probably never
// https://github.com/dotnet/standard/issues/1567
// maybe no point bothering with uwp

// But other than targeting HoloLens and Xbox, there's no reason to use UWP.
// WinRT will remain alive and well, and is seeing lots of investment, but the investment is in bringing the rich functionality that WinRT in UWP provided to regular Win32 applications.

// Microsoft tried to get Windows developers to adopt its UWP model but too many of them refused – for all sorts of reasons.
// The result is that Redmond has had to backtrack, unfortunately at the expense of the keen minority who did adopt it.

// HoloLens doesn't use UWP any longer, rather OpenXR.
// XBox is now pushing for GDK for anyone that wants a go at its app store, leaving UWP for community only, unsupported projects. GDK also documents the WinRT based framework (ERA) as legacy.
// Forms, WPF and Win32 are indeed the only way forward to all of us burned by rewrites.
// Even their own teams rather use Electron!
// WinRT is COM version 2 and COM itself is essentially a native object exposing mechanism and has nothing to do either with UWP or Win32 or Any kind of any AppModels whatsoever in any way shape or form.

// https://www.theregister.com/2021/07/02/uwp_microsoft_winui3/

// WinUI 3 is a new 3rd generation of the native UX stack in Windows. It consolidates the UX technologies previously built into Windows into a single decoupled framework that ships as part of the Windows App SDK, previously known as Project Reunion."
// At one time, the story was that UWP developers would not miss out, since WinUI 3 would be available both for desktop and UWP. This was part of the "reunion." That plan has now changed, even though the docs referenced above still say "WinUI 3 support for production-ready UWP apps is currently in preview."
// 
// the long-term future of UWP now looks bleak.

// https://www.theregister.com/2021/03/30/microsoft_05_project_reunion/

// Microsoft has released an early version of Project Reunion with support for production use – provided developers do not target Universal Windows Platform (UWP).
// 
// https://www.theregister.com/2021/10/22/microsoft_net_hot_reload_visual_studio/?td=keepreading-top
// Microsoft has enraged the open-source .NET community by removing flagship functionality from open-source .NET to bolster the appeal of Visual Studio, not least against its cross-platform cousin Visual Studio Code.
// 2,500 lines of code implementing a feature called Hot Reload are removed from a tool called dotnet watch; and this blog post in which Principal Program Manager Dmitry Lyalin revealed "we’ve decided that starting with the upcoming .NET 6 GA release, we will enable Hot Reload functionality only through Visual Studio 2022."
// Hot Reload is a feature whereby developers can modify source code while an application is running, apply the changes, and see the results in the running application. It speeds the development process because it is quicker than rebuilding the code, stopping the application, applying the changes, and then firing it up again.

// When Hot Reload was introduced Lyalin said developers could use it through "the .NET Hot Reload experience in Visual Studio 2019 version 16.11 (Preview 1) and through the dotnet watch command-line tooling in .NET 6 (Preview 4)." The feature is in .NET 6 RC2, which has a go-live license, and which was released on October 12 ahead of the launch of .NET 6 planned for November 9 at the online .NET Conf 2021.
// This is the PR that is truly going to decide .NET is really an OSS [open source software project] or not," said one developer.
// It appears Microsoft has changed its mind in light of the uproar from developers, and Hot Reload isn't being removed from open-source .NET.



namespace WinUINotifsMSIX.Models
{
    public class BettingContext : DbContext
    {
        // https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext?view=efcore-3.1

        // A DbContext instance represents a session with the database and can be used to query and save instances of your entities. DbContext is a combination of the Unit Of Work and Repository patterns.
        // Typically you create a class that derives from DbContext and contains DbSet<TEntity> properties for each entity in the model. 


        // 20200523
        // https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-strings#universal-windows-platform-uwp

        // https://docs.microsoft.com/en-au/ef/core/get-started/?tabs=netcore-cli
        // To allow reverse engineering so that this project's model is the same as the db schema I installed
        // this workload: 
        // .NET Core cross-platform development(under Other Toolsets)
        // https://docs.microsoft.com/en-us/ef/core/managing-schemas/scaffolding?tabs=vs

        // https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/powershell
        // If you're using Visual Studio, we recommend the Package Manager Console tools instead:
        // but this seems to relate to ASP NET CORE
        // Tried PMC

        // Get-DbContext
        // Startup project 'UWPExampleCS' is a Universal Windows Platform app. 
        // This version of the Entity Framework Core Package Manager Console Tools doesn't support this type of project. For more information on using the EF Core Tools with UWP projects, see https://go.microsoft.com/fwlink/?linkid=858496

        // which is
        // https://docs.microsoft.com/en-au/ef/core/get-started/?tabs=netcore-cli
        // back to where I started
        // https://docs.microsoft.com/en-au/ef/core/managing-schemas/scaffolding?tabs=dotnet-core-cli
        //  dotnet ef dbcontext scaffold
        // also fails with same  MSB4019 error as below in cli and in PMC Scaffold-DbContext says UWP no good as above.
        // so will try Fluent API manually instead. See below.

        // https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet

        // 20200524
        // Gave up on getting CLI tools to work for now. See errors below.
        // Package Manager Console tools also no good



        // command-line interface (CLI) tools for Entity Framework Core perform design-time development tasks. 
        // For example, they create migrations, apply migrations, and 
        // generate code for a model based on an existing database.
        // The commands are an extension to the cross-platform dotnet command, which is part of the .NET Core SDK
        // These tools work with .NET Core projects.
        // dotnet ef must be installed as a global or local tool. Most developers will install dotnet ef as a global tool with the following command:
        // dotnet tool install --global dotnet-ef

        // At 20200523 I went to Tools/command line powershell and it loaded the VS 2019 Powershell 
        // I ran the global install cmd and it said

        // You can invoke the tool using the following command: dotnet-ef
        // Tool 'dotnet-ef' (version '3.1.4') was successfully installed.

        // it is .NET Core CLI
        // dotnet --info
        // showed that I already have the net core sdk, so didn't need this
        // https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.300-windows-x64-installer
        // did this
        // dotnet add package Microsoft.EntityFrameworkCore.Design
        // but error
        // so installed it via Manage nuget packages instead.

        // I should now have all the tools for EF
        // ran 
        // dotnet tool update --global dotnet-ef
        // and it said: Tool 'dotnet-ef' was reinstalled with the latest stable version (version '3.1.4').

        // maybe will have cli problems due to Apps that have the EF Core model in a .NET Standard class library might not have a .NET Core or .NET Framework project. 

        // dotnet ef dbcontext info
        // failed with:
        // No project was found. Change the current working directory or use the --project option.
        //  dotnet ef dbcontext info --project UWPExampleCS
        // gave:
        // error MSB4019: The imported project "C:\Program Files\dotnet\sdk\3.1.300\Microsoft\WindowsXaml\v16.0\Microsoft.Windows.UI.Xaml.CSharp.targets" was not found.
        // Confirm that the expression in the Import declaration "C:\Program Files\dotnet\sdk\3.1.300\\Microsoft\WindowsXaml\v16.0\Microsoft.Windows.UI.Xaml.CSharp.targets" is correct, and that the file exists on disk.

        // Well, it didn't exist on disk, as the C:\Program Files\dotnet\sdk\3.1.300\Microsoft folder only has Microsoft.NET.Build.Extensions
        // https://stackoverflow.com/questions/45041972/microsoft-windows-ui-xaml-csharp-targets-missing-on-ci-server
        // https://github.com/Microsoft/MixedRealityToolkit-Unity/issues/1009#issuecomment-332235474
        // remove the MSBuild tools for VS2017, and then rerun the VS2017cleanup/installer and reboot to fix this
        // https://stackoverflow.com/questions/5694/the-imported-project-c-microsoft-csharp-targets-was-not-found
        // I have UWP Tools installed as part of UWP
        // http://www.csharp411.com/microsoft-csharp-targets-was-not-found/
        // Open the current Visual Studio project file (with the .csproj extension) in Notepad.
        // <Import Project="$(MSBuildToolsPath)Microsoft.CSharp.targets" /> 
        // replace with
        // <Import Project="$(MSBuildBinPath)Microsoft.CSharp.targets" /> 

        // mine actually has
        // <Import Project="$(MSBuildExtensionsPath)\Microsoft\WindowsXaml\v$(VisualStudioVersion)\Microsoft.Windows.UI.Xaml.CSharp.targets" />
        // I removed that but that caused 
        // error MSB4057: The target "GetEFProjectMetadata" does not exist in the project.
        // and also VS couldn't load the project, so put it back in.

        // https://xamlbrewer.wordpress.com/2016/06/01/getting-started-with-sqlite-and-entity-framework-on-uwp/

        // https://devblogs.microsoft.com/dotnet/announcing-entity-framework-core-3-1-and-entity-framework-6-4/

        // Starting in 3.0 and continuing for 3.1, the dotnet ef command-line tool is no longer included in the .NET Core SDK.
        // Before you can execute EF Core migration or scaffolding commands, you’ll have to install this package as either a global or local tool.
        // To install the final version of our 3.1.0 tool as a global tool, use the following command:
        // dotnet tool install --global dotnet-ef --version 3.1.0
        // A commenter said: I could not get ef migrations working on 3.1.

        // 20200524

        // Fluent API
        // Decided to use Fluent API. See onModelCreating below.

        // https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/fluent/types-and-properties
        // is std EF docmn for fluent and this presumably still applies to ef core.

        // https://stackoverflow.com/questions/46432074/how-to-use-a-dbcontext-in-a-static-class-objectdisposedexception
        // the static nature of an extension is completely incompatible with the concept of a EF context object. 
        // Never declare DbContext as static, it will cause all sorts of trouble, and not refresh the data, so you will be getting old data from a query.


        // https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext?view=efcore-3.1

        // set in app ctor
        public BettingContext(DbContextOptions<BettingContext> options)
      : base(options)
        {
            Log.Debug("Entered with options arg");
        }

        // https://codereview.stackexchange.com/questions/120835/business-with-dbcontext-and-static-class
        // Making the class static pretty much does that for you, and prevents anyone from inheriting the class and/or calling its constructor.
        // the EF DbContext is a powerful unit-of-work. Yes, an EF DbSet<T> is a nice repository.

        // https://stackoverflow.com/questions/55530955/optimize-connection-to-sqlite-db-using-ef-core-in-uwp-app
        // I tried to create a single DbContext once statically and share it across my entire application.
        //  this is not thread safe

        // I think, you can make the DBContext object global and static and when you want to do CRUD, you can do it in main thread like this:
        // But do dispose the DBContext at a proper time as it won't automatically get disposed. 

        public BettingContext()
        {
            Log.Debug("Entered with no args");
        }
        // 20200630
        // this is db related so we only deal with Models.Notification here. No vm refcs
        internal List<Notification> GetStoredNotifications(Byte Thresh)
        {
            try
            {
                Log.Verbose("Entered with notification threshold = {t}", Thresh);
                var seln = Notifications.Where(n => n.Visibility >= Thresh).OrderBy(n => n.CreationTime).ToList();
                return seln;
             
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }
        public IQueryable<Notification> GetDBNotifications(int VisibilityThreshold)
        { 
            var DBNotifications = new List<Models.Notification>();
            try
            {
                DBNotifications = GetStoredNotifications((byte)VisibilityThreshold);
                Log.Information("Found {s} notifications on the database with a visibility >= visibility threshold of {t}", DBNotifications.Count, VisibilityThreshold);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                //StatusMessage = ex.Message;
            }
            return DBNotifications.AsQueryable();
        }
        // 20200715
        internal Notification? GetStoredNotification(int ID)
        {
            try
            {
                Log.Verbose("Entered with notification id = {t}", ID);
                Notification? seln = Notifications.FirstOrDefault(n => n.Id == ID);
                return seln;

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }
        public void DeleteNotification(int ID)
        {
            try
            {
                //Log.Verbose("Entered with notification id = {t}", ID);
                Notifications.RemoveRange(Notifications.Where(n => n.Id == ID));
                SaveChanges();
                //return MET;

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }
        public void WriteNotification(NotificationItem notif)
        {
            // 20200602
            // write new notif to db so that db matches vm
            // 20200715
            // Now that we refresh the vm the notif may already exist on db and if so, do not store it
            // not actually needed since I have fixed adding to the vm but will keep code anyway 

            try
            {

                Log.Information("Writing Notification ID={i} to the database", notif.ID);

                // 20200715
                Notification? DBNotification = GetStoredNotification((int)notif.ID);

                if (DBNotification is null)
                {
                    DBNotification = new Notification
                    {
                        Id = (int)notif.ID,
                        Body = notif.Body,
                        Title = notif.Title,
                        CreationTime = notif.CreationTime.LocalDateTime,
                        Source = notif.Source,
                        Visibility = (byte)notif.Visibility,
                        TS = DateTime.Now
                    };

                    Add(DBNotification);
                    SaveChanges();

                }
                else
                // it already exists on db so update it to have the new visibility
                {
                    // 1st param is current vm value
                    Log.Information("Notification with id {id} already exists on db with Visibility {ov} so calling UpdateDBVisibility to update it to new visibilty {b} if it differs", notif.ID, DBNotification.Visibility, notif.Visibility);
                    UpdateDBVisibility(DBNotification.Visibility, (byte)notif.Visibility, (int)notif.ID);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }
        public void UpdateDBVisibility(int? visibility, int? newvisibility, int id)
        {
            try
            {
                if (newvisibility != visibility && visibility != null)
                {
                    Log.Debug("DB updating follows for id {id}");

                    var tracked = Notifications.FirstOrDefault(n => n.Id == id);
                    if (tracked != null)
                    {
                        tracked.Visibility = (byte)newvisibility;
                        tracked.TS = DateTime.Now;

                        SaveChanges();
                        Log.Information("Visibility was updated to {c} for the Notification table entry with ID={id}", newvisibility, id);
                    }
                    else
                    {
                        Log.Warning("No tracked Notification found with ID={id}", id);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }


        // 20220526
        internal MatchEventType? GetMatchEventType(string EventType)
        {
            try
            {
                //Log.Verbose("Entered with notification id = {t}", ID);
                MatchEventType? MET = MatchEventTypes.FirstOrDefault(n => n.EventType == EventType);
                return MET;

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

     

        // 20220526
        internal tblAFLMatchEvent? GetMatchEvent(long MatchEventTypeID,int AFLMatchID,DateTime EventUTC)
        {
            try
            {
                //Log.Verbose("Entered with notification id = {t}", ID);
                tblAFLMatchEvent? ME = tblAFLMatchEvents.FirstOrDefault(n => n.MatchEventTypeID == MatchEventTypeID && n.AFLMatchID == AFLMatchID && n.EventUTC == EventUTC);
                return ME;

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        // 20220524
        // trying to keep all db access in this class

        internal IQueryable<tblAFLMatch> GetAFLMatches(DateTime NotificationTime, int FutureMinutes = -480, int PastMinutes = 15)
        {
            try
            {
                //Log.Verbose("Entered with notification id = {t}", ID);

                // 20220525
                // For some crazy reason I made Notification table dates local rather than UTC.
                // Until I fix the db, will cater for it here.
                // Expect notifs to be between 480 mins before match and 5 mins after it
                // so look 480 mins after the notif and 5 mins before it
                // so notif < match+5 and notif > match-480

                // made it 15 mins in future after port v ess was 8 mins

                // The notif has info in it that will also help find the associated match #FreoCats etc. but can add that later.
                //int FutureMinutes = -480;
                //int PastMinutes = 15;
                IQueryable<tblAFLMatch> aflmatches =  tblAFLMatches.Where(x => NotificationTime.ToUniversalTime() > x.StartDateGMT.AddMinutes(FutureMinutes) && NotificationTime.ToUniversalTime() < x.StartDateGMT.AddMinutes(PastMinutes)  );
                Log.Information("For NotificationTime {t} found {z} matches within {a} and {m} mins", NotificationTime, aflmatches.Count(), FutureMinutes, PastMinutes);
                return aflmatches;

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }
        internal int GetTeamID(string TeamName)
        {
            try
            {

                int TeamID = 0;
                int rc = 0;

                // https://erikej.github.io/efcore/2020/08/03/ef-core-call-stored-procedures-out-parameters.html
                // https://stackoverflow.com/questions/26823148/ef6-executesqlcommandasync-get-return-parameter-declare-scalar-variable-err

                // For some reason need output for return value!!
                // Don't need @ in defs here

                var paramrc = new SqlParameter
                {
                    ParameterName = "rc",
                    SqlDbType = System.Data.SqlDbType.Int,
                    //  Direction = System.Data.ParameterDirection.ReturnValue,
                    Direction = System.Data.ParameterDirection.Output,
                };

                SqlParameter paramTeamName = new SqlParameter("TeamName", TeamName);

                var paramTeamID = new SqlParameter
                {
                    ParameterName = "TeamID",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };


                // https://referbruv.com/blog/working-with-stored-procedures-in-aspnet-core-ef-core/
                // the return value xx isn't the return result of the query, but the number of records returned [-1 here]

                var RowsCount = Database.ExecuteSqlRaw("EXECUTE @rc = dbo.uspGetTeamIDFromName @TeamName, @TeamID OUTPUT", paramrc, paramTeamName, paramTeamID);

                if (RowsCount != -1)
                    throw new ApplicationException(String.Format("Non -1 Rows {0} returned from uspGetTeamIDFromName for Team {1}", RowsCount, TeamName));

                rc = (int)paramrc.Value;

                if (rc != 0)
                    throw new ApplicationException(String.Format("Non zero rc {0} returned from uspGetTeamIDFromName for Team {1}", rc, TeamName));

                // rc=6292 is not found

                TeamID = (int)paramTeamID.Value;

                return TeamID;

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }

        }


        internal tblAFLMatchEvent? WriteMatchEvent(long MatchEventTypeID, int AFLMatchID, DateTime creationTime, string matchEventText)
        {
            try
            {

                // validate dup key first by read rather than test the exception of dup key
                tblAFLMatchEvent? me = GetMatchEvent(MatchEventTypeID, AFLMatchID, creationTime.ToUniversalTime());

                if (me != null)
                {
                    // would be dup key, so use null to show this
                    me = null;
                }
                else
                {
                    me = new tblAFLMatchEvent
                    {
                        MatchEventTypeID = MatchEventTypeID,
                        AFLMatchID = AFLMatchID,
                        EventUTC = creationTime.ToUniversalTime(),
                        Description = matchEventText
                    };

                    Add(me);
                    SaveChanges();
                }

                return me;
            }
            catch (Exception ex)
            {
                Log.Error("fail", ex);
                throw;
            }
        }
        public async Task<List<SeriLog>> GetRecentLogsAsync(DateTime? since = null, string? applicationFilter = null)
        {
            try
            {
                var query = SeriLogs.AsQueryable();

                if (since.HasValue)
                    query = query.Where(l => l.TimeStamp > since.Value);

                if (!string.IsNullOrWhiteSpace(applicationFilter) && applicationFilter != "All")
                    query = query.Where(l => l.Application == applicationFilter);

                var logs = await query
                    .OrderBy(l => l.Id)
                    .Take(1000)
                    .ToListAsync();

                return logs;
            }
            catch (SqlException ex)
            {
                Log.Error(ex, "SQL error in GetRecentLogsAsync: {Message}", ex.Message);
                return new List<SeriLog>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetRecentLogsAsync: {Message}", ex.Message);
                return new List<SeriLog>();
            }
        }
        public async Task<List<string?>> GetRecentApplicationsAsync(int maxRows = 10000)
        {
            try
            {
                var apps = await SeriLogs
                    .OrderByDescending(l => l.Id)
                    .Take(maxRows)
                    .Select(l => l.Application)
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Distinct()
                    .OrderBy(a => a)
                    .ToListAsync();

                return apps;
            }
            catch (SqlException ex)
            {
                Log.Error(ex, "SQL error in GetRecentApplicationsAsync: {Message}", ex.Message);
                return new List<string?>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetRecentApplicationsAsync: {Message}", ex.Message);
                return new List<string?>();
            }
        }

        // https://docs.microsoft.com/en-us/ef/core/modeling/entity-types?tabs=data-annotations#including-types-in-the-model

        // Including a DbSet of a type on your context means that it is included in EF Core's model
        // we usually refer to such a type as an entity.
        // By convention, types that are exposed in DbSet properties on your context are included in the model as entities. 
        // Entity types that are specified in the OnModelCreating method are also included, as are any types that are found by recursively exploring the navigation properties of other discovered entity types.
        // If you don't want a type to be included in the model, you can exclude it:
        // modelBuilder.Ignore<BlogMetadata>();

        // By convention, each entity type will be set up to map to a database table with the same name as the DbSet property that exposes the entity. If no DbSet exists for the given entity, the class name is used.
        // You can manually configure the table name:
        // entity properties map to table columns.
        // By convention, all public properties with a getter and a setter will be included in the model.
        // By convention, when using a relational database, entity properties are mapped to table columns having the same name as the property.
        // .HasColumnName("blog_id");

        // Properties
        // If the DbSet<TEntity> properties have a public setter, they are automatically initialized when the instance of the derived context is created. 

        // nb Notes is only used in EFCoreSetup for EF testing purposes. It isn't actually used.

        public DbSet<Notes> Notes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<tblAFLMatch> tblAFLMatches { get; set; }
        public DbSet<MatchEventType> MatchEventTypes { get; set; }
        public DbSet<tblAFLMatchEvent> tblAFLMatchEvents { get; set; }
        public DbSet<SeriLog> SeriLogs { get; set; }

        // copied from jstdlog
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext?view=efcore-3.1
            // The model is discovered by running a set of conventions over the entity classes found in the DbSet<TEntity> properties on the derived context.
            // To further configure the model that is discovered by convention, you can override the OnModelCreating(ModelBuilder) method. 

            // https://stackoverflow.com/questions/52738995/when-does-efcore-onmodelcreating-get-invoked

            // OnModelCreating is called by the framework when it is created the first time to make the models and map into memory.
            // The method is called on program start, in order that EF can validate the current state of the database. 

            // https://stackoverflow.com/questions/33076291/onmodelcreating-is-never-called
            // Typically, this method is called only once when the first instance of a derived context is created. 
            // https://stackoverflow.com/questions/42627952/entity-framework-core-onmodelcreating-vs-migration-the-correct-development-work
            // OnModelCreating is necessary when you have to tell EF how the entities map to a database. 

            // https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/fluent/types-and-properties
            // https://docs.microsoft.com/en-us/ef/core/modeling/

            //     modelBuilder.Entity<Post>()
            //.HasOne(p => p.Blog)
            //.WithMany(b => b.Posts)
            //.HasForeignKey(p => p.BlogUrl)
            //.HasPrincipalKey(b => b.Url);

            //  .HasAlternateKey(c => c.LicensePlate);

            //         .HasRequired(c => c.Department)
            //.WithMany(t => t.Courses)
            //.Map(m => m.MapKey("ChangedDepartmentID"));

            // .Map<Course>(m => m.Requires("Type").HasValue("Course"))  

            //          modelBuilder
            //.Entity<Blog>()
            //.MapToStoredProcedures();

            try
            {
                Log.Debug("Entered");

                Log.Verbose("Model.GetDefaultSchema = {s}", modelBuilder.Model.GetDefaultSchema());

                // Notes table

                modelBuilder.Entity<Notes>()
                    .Property(b => b.Id)
                    .UseIdentityColumn()
                    .IsRequired();

                modelBuilder.Entity<Notes>(entity =>
                {
                    entity.HasKey(c => new { c.Id });
                });

                modelBuilder.Entity<Notes>().Property(t => t.Comments).HasMaxLength(1000).IsRequired().HasColumnType("varchar(1000)");

                // https://docs.microsoft.com/en-us/ef/core/modeling/generated-properties?tabs=fluent-api#value-generated-on-add-1
                modelBuilder.Entity<Notes>().Property(t => t.TS).IsRequired().HasDefaultValueSql("getdate()").HasColumnType("datetime");

                // https://entityframeworkcore.com/knowledge-base/48267925/invalid-object-name-error---entityframeworkcore-2-0
                modelBuilder.Entity<Notes>().ToTable("Notes");

                // Notification table

                modelBuilder.Entity<Notification>()
                    .Property(b => b.Id)
                    .IsRequired();

                modelBuilder.Entity<Notification>(entity =>
                {
                    entity.HasKey(c => new { c.Id });
                });

                modelBuilder.Entity<Notification>()
                  .Property(b => b.CreationTime)
                  .IsRequired().HasColumnType("datetime");

                modelBuilder.Entity<Notification>()
                   .Property(b => b.Source)
                   .HasMaxLength(100)
                   .IsRequired();

                modelBuilder.Entity<Notification>().Property(t => t.Title).HasMaxLength(100).IsRequired().HasColumnType("varchar(100)");
                modelBuilder.Entity<Notification>().Property(t => t.Body).HasMaxLength(2048).HasColumnType("nvarchar(2048)"); // 20200826 - nvarchar for emojis etc.

                // https://docs.microsoft.com/en-us/ef/core/modeling/generated-properties?tabs=fluent-api#value-generated-on-add-1
                // https://stackoverflow.com/questions/49634598/how-to-use-valuegeneratedonupdate-correctly
                // doesn't seem to work without manually creating a trigger??
                // I set TS manually now instead
                modelBuilder.Entity<Notification>().Property(t => t.TS).IsRequired().HasColumnType("datetime");
//                modelBuilder.Entity<Notification>().Property(t => t.TS).IsRequired().HasDefaultValueSql("getdate()").HasColumnType("datetime").ValueGeneratedOnUpdate();
                //            modelBuilder.Entity<Notification>().Property(t => t.TS).IsRequired().HasDefaultValueSql("getdate()").ValueGeneratedOnAddOrUpdate();

                // 20200617
                modelBuilder.Entity<Notification>().Property(t => t.Visibility).IsRequired().HasColumnType("tinyint");


                // https://entityframeworkcore.com/knowledge-base/48267925/invalid-object-name-error---entityframeworkcore-2-0

                modelBuilder.Entity<Notification>().ToTable("Notification");

                // https://blog.pieeatingninjas.be/2017/09/02/using-sqlclient-in-uwp/


                // 20220524
                // tblAFLMatch table
                // omits preds

                modelBuilder.Entity<tblAFLMatch>()
                    .Property(b => b.ID)
                    .UseIdentityColumn()
                    .IsRequired();

                modelBuilder.Entity<tblAFLMatch>(entity =>
                {
                    entity.HasKey(c => new { c.ID });
                });

                modelBuilder.Entity<tblAFLMatch>().Property(t => t.AFLMatchTypeID).IsRequired().HasColumnType("int");
                modelBuilder.Entity<tblAFLMatch>().Property(t => t.AFLRoundID).IsRequired().HasColumnType("int");
                modelBuilder.Entity<tblAFLMatch>().Property(t => t.MatchNumber).IsRequired().HasColumnType("tinyint");
                modelBuilder.Entity<tblAFLMatch>().Property(t => t.VenueID).HasColumnType("int");
                modelBuilder.Entity<tblAFLMatch>().Property(t => t.MatchName).HasMaxLength(50).IsRequired().HasColumnType("varchar(50)");
                modelBuilder.Entity<tblAFLMatch>().Property(t => t.StartDateGMT).HasColumnType("smalldatetime");
                modelBuilder.Entity<tblAFLMatch>().Property(t => t.MatchStatus).HasMaxLength(1).IsRequired().HasColumnType("char(1)");

                // https://docs.microsoft.com/en-us/ef/core/modeling/generated-properties?tabs=fluent-api#value-generated-on-add-1
                modelBuilder.Entity<tblAFLMatch>().Property(t => t.TS).IsRequired().HasDefaultValueSql("getdate()").HasColumnType("smalldatetime");

                // https://entityframeworkcore.com/knowledge-base/48267925/invalid-object-name-error---entityframeworkcore-2-0
                modelBuilder.Entity<tblAFLMatch>().ToTable("tblAFLMatch");

                // 20220526
                // MatchEventType table
                // doesn't have TS

                modelBuilder.Entity<MatchEventType>()
                    .Property(b => b.ID)
                    .UseIdentityColumn()
                    .IsRequired().HasColumnType("bigint");

                modelBuilder.Entity<MatchEventType>(entity =>
                {
                    entity.HasKey(c => new { c.ID });
                });
                
                modelBuilder.Entity<MatchEventType>().Property(t => t.EventType).HasMaxLength(4).IsRequired().HasColumnType("char(4)");
                modelBuilder.Entity<MatchEventType>().Property(t => t.EventTypeDescription).HasMaxLength(100).HasColumnType("varchar(100)");
          
                modelBuilder.Entity<MatchEventType>().ToTable("MatchEventType");

                // 20220526
                // tblAFLMatchEventType table
                
                modelBuilder.Entity<tblAFLMatchEvent>()
                    .Property(b => b.ID)
                    .UseIdentityColumn()
                    .IsRequired().HasColumnType("int");

                modelBuilder.Entity<tblAFLMatchEvent>(entity =>
                {
                    entity.HasKey(c => new { c.ID });
                });

                modelBuilder.Entity<tblAFLMatchEvent>().Property(t => t.MatchEventTypeID).IsRequired().HasColumnType("bigint");
                modelBuilder.Entity<tblAFLMatchEvent>().Property(t => t.AFLMatchID).IsRequired().HasColumnType("int");
                modelBuilder.Entity<tblAFLMatchEvent>().Property(t => t.EventUTC).IsRequired().HasColumnType("datetime");
                modelBuilder.Entity<tblAFLMatchEvent>().Property(t => t.Description).IsRequired().HasMaxLength(300).HasColumnType("varchar(300)");
                modelBuilder.Entity<tblAFLMatchEvent>().Property(t => t.TS).IsRequired().HasDefaultValueSql("getdate()").HasColumnType("datetime");

                modelBuilder.Entity<tblAFLMatchEvent>().ToTable("tblAFLMatchEvent");

                // keyless entity - copied from IncompletePoolMatch

                //modelBuilder.Entity<TeamKey>().HasNoKey();

                modelBuilder.Entity<SeriLog>()
                .Property(b => b.Id).UseIdentityColumn()
                .IsRequired().HasColumnType("int");

                modelBuilder.Entity<SeriLog>(entity =>
                {
                    entity.HasKey(c => new { c.Id });
                });

                modelBuilder.Entity<SeriLog>().Property(b => b.Message).HasColumnType("nvarchar(max)");
                modelBuilder.Entity<SeriLog>().Property(b => b.MessageTemplate).HasColumnType("nvarchar(max)");
                modelBuilder.Entity<SeriLog>().Property(b => b.Level).HasColumnType("nvarchar(max)");
                modelBuilder.Entity<SeriLog>().Property(b => b.TimeStamp).HasColumnType("datetime");
                modelBuilder.Entity<SeriLog>().Property(b => b.Exception).HasColumnType("nvarchar(max)");
                modelBuilder.Entity<SeriLog>().Property(b => b.Properties).HasColumnType("nvarchar(max)");
                modelBuilder.Entity<SeriLog>().Property(b => b.LogEvent).HasColumnType("nvarchar(max)");
                modelBuilder.Entity<SeriLog>().Property(b => b.Application).HasColumnType("nvarchar(40)");
                modelBuilder.Entity<SeriLog>().Property(b => b.MethodName).HasColumnType("nvarchar(120)");


                // 20200602 - added index
                modelBuilder.Entity<SeriLog>().ToTable("SeriLog")
                                              .HasIndex(s => new { s.TimeStamp }).IsClustered(false);


            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }


            // https://stackoverflow.com/questions/42627952/entity-framework-core-onmodelcreating-vs-migration-the-correct-development-work
            // OnModelCreating is necessary when you have to tell EF how the entities map to a database. 

        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            try
            {
            // Override the OnConfiguring(DbContextOptionsBuilder) method to configure the database (and other options) to be used for the context. 
            // Alternatively, if you would rather perform configuration externally instead of inline in your context, you can use DbContextOptionsBuilder<TContext> (or DbContextOptionsBuilder) 
            // to externally create an instance of DbContextOptions<TContext> (or DbContextOptions) and pass it to a base constructor of DbContext. 

            Log.Debug("Entered with IsConfigured state = {s}", optionsBuilder.IsConfigured);

            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.onconfiguring?view=efcore-3.1
            // https://stackoverflow.com/questions/59535471/how-do-i-set-the-onconfiguring-method-in-my-dbcontext-without-any-hardcoded-conn
            // https://stackoverflow.com/questions/39058739/dbcontext-onconfiguring-not-getting-called-and-behaving-weird-in-asp-net-core
            // It runs whenever a context instance is actually used for the first time. 

            // This method is called for each instance of the context that is created. 
            // The base implementation does nothing. 
            // want change tracking since it is needed for SaveChanges() in theory but it did work ??

            // so, in app.efcoresetup context is created with args and OnConfiguring and OnModelCreating are both called
            // in mainpage when using dbcontext with no args, just OnConfiguring is called

            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(App.BettingConnectionString, providerOptions => providerOptions.CommandTimeout(30));
            //               .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
   
         
        }

    }
}
