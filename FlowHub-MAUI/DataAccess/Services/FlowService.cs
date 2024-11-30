using Parse.Abstractions.Platform.Authentication;
using Parse.LiveQuery;
using System.Diagnostics;

namespace FlowHub_MAUI.DataAccess.Services;

public class FlowService : IFlowsService
{
    Realm? db;
    public IList<FlowsModelView>? AllFlows { get; set ; }
    public IList<FlowCommentsView>? AllFlowComments { get; set ; }
    public IList<FlowModelAndCommentLink>? AllFlowAndCommentsLink { get; set ; }
    HomePageVM? ViewModel { get; set; }
    public IDataBaseService DataBaseService { get; }

    public FlowService(IDataBaseService dataBaseService)
    {
        DataBaseService = dataBaseService;

        GetUserAccount();
        GetFlows();
    }


    //bool HasOnlineSyncOn;
    public ParseUser? CurrentUserOnline { get; set; }

    public UserModelView? CurrentOfflineUser { get; set; }
    public void InitApp(HomePageVM vm)
    {
        ViewModel = vm;
        
    }

    public UserModelView? GetUserAccount(ParseUser? usr = null)
    {
        // If current offline user is already authenticated and no user is passed, return it
        if (CurrentOfflineUser is not null && CurrentOfflineUser.IsAuthenticated && usr is null)
        {
            return CurrentOfflineUser;
        }

        // Access the Realm database instance
        db = Realm.GetInstance(DataBaseService.GetRealm());

        // Attempt to find the existing user in the database
        var dbUser = db.All<UserModel>().FirstOrDefault();

        if (dbUser is not null)
        {
            // Convert the database user to the current offline user
            CurrentOfflineUser = new UserModelView(dbUser);
            return CurrentOfflineUser;
        }

        // If no existing user is found in the database
        if (usr is not null)
        {
            // Create a new UserModelView from the provided ParseUser
            CurrentOfflineUser = new UserModelView(usr);

            // Write the new user to the database
            db.Write(() =>
            {
                var user = new UserModel(CurrentOfflineUser);
                db.Add(user, update: true); // Update if user already exists
            });

            return CurrentOfflineUser;
        }

        // Create a default offline user if no user is found or provided
        CurrentOfflineUser = CreateDefaultOfflineUser();

        // Save the default user to the database
        db.Write(() =>
        {
            var user = new UserModel(CurrentOfflineUser);
            db.Add(user, update: false); // Do not update; create new
        });

        return CurrentOfflineUser;
    }

    /// <summary>
    /// Creates a default offline user.
    /// </summary>
    private UserModelView CreateDefaultOfflineUser()
    {
        return new UserModelView
        {
            UserName = "User",
            UserEmail = "user@FlowHub.com",
            UserPassword = "1234"
        };
    }


    public async Task<bool> SignUpUser(ParseUser user)
    {
        try
        {
            _ = ParseClient.Instance.SignUpAsync(user);
            // notify to verify email
            await Shell.Current.DisplayAlert("Last Step!", "Please Verify Your Email!", "Ok");
            _ = ParseClient.Instance.LogOutAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error when registering user: " + ex.Message);
            return false;
        }
    }

    public static void LogUserOut()
    {
        _ = ParseClient.Instance.LogOutAsync();
    }
    public async Task<bool> LogUserIn(ParseUser user, string password)
    {

        CurrentUserOnline = await ParseClient.Instance.LogInAsync(user.Username, password);
        //var isVerified =(bool)CurrentUserOnline["emailVerified"] == true;
        //if (isVerified)
        //{
        //    await Shell.Current.DisplayAlert("Success!", "Logged In!", "Ok");
        //}
        //else
        //{
        //    await Shell.Current.DisplayAlert("Error!", "Please Verify Your Email!", "Ok");
        //    return false;
        //}

        return true;
    }
    public void GetFlows()
    {
        try
        {
            db = Realm.GetInstance(DataBaseService.GetRealm());
            AllFlows?.Clear();
            var realflows = db.All<FlowsModel>().OrderBy(x=>x.DateCreated).ToList();
            db.Write(() =>
            {
                AllFlows = new List<FlowsModelView>(realflows.Select(flow => new FlowsModelView(flow)));

            });
            AllFlows ??= Enumerable.Empty<FlowsModelView>().ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public string AddFlowComment(FlowCommentsView comment)
    {
        try
        {
            db = Realm.GetInstance(DataBaseService.GetRealm());
            string localDeviceId = string.Empty;
            db.Write(() =>
            {
                var existingFlow = db.All<FlowComments>()
                    .Where(x => x.LocalDeviceId == comment.LocalDeviceId)
                    .ToList();
                if (existingFlow.Count < 1)
                {
                    FlowComments flow = new FlowComments(comment);
                    db.Add(flow);
                    localDeviceId = flow.LocalDeviceId!;
                    Debug.WriteLine("OK Saved flow db add");
                }
                else
                {
                    FlowComments dbFlow = new FlowComments(comment);
                    db.Add(dbFlow, update: true);
                    localDeviceId = dbFlow.LocalDeviceId!;
                    Debug.WriteLine("OK Saved flowc db up");
                }
            });

            if (CurrentUserOnline is not null && CurrentUserOnline.IsAuthenticated)
            {
                _ = SendSingleObjectToParse(nameof(FlowCommentsView), comment);
                Debug.WriteLine("OK Saved flow online add");
            }

            return localDeviceId;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error when updating song: " + ex.Message);
            return string.Empty;
        }
    }
    public void SaveBoth(FlowsModelView flow, FlowCommentsView comment)
    {
        try
        {
            
            var fCommentId= AddFlowComment(comment);
            flow.FlowModelCommentLinkID = fCommentId;
            
            UpdateFlow(flow);

            FlowModelAndCommentLink link = new()
            {
                FlowID = flow.LocalDeviceId,
                CommentID = fCommentId
            };
            
            AddFlowModelAndCommendlink(link);

        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error when updating song: " + ex.Message);
            
        }
    }

    public string AddFlowModelAndCommendlink(FlowModelAndCommentLink link)
    {
        try
        {
            db = Realm.GetInstance(DataBaseService.GetRealm());
            string localDeviceId = string.Empty;
            db.Write(() =>
            {
                db.Add(link);
                localDeviceId = link.LocalDeviceId!;
                Debug.WriteLine("OK Saved link db");
            });


            if (CurrentUserOnline is not null && CurrentUserOnline.IsAuthenticated)
            {
                _ = SendSingleObjectToParse(nameof(FlowModelAndCommentLink), link);
                Debug.WriteLine("OK Saved link online");
            }
            Debug.WriteLine("OK Saved link okk");
            return localDeviceId;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error when updating song: " + ex.Message);
            return string.Empty;
        }
    }

    public string UpdateFlow(FlowsModelView flow)
    {
        try
        {
            db = Realm.GetInstance(DataBaseService.GetRealm());
            string localDeviceId = string.Empty;
            db.Write(() =>
            {
                var existingFlow = db.All<FlowsModel>()
                .Where(x => x.LocalDeviceId == flow.LocalDeviceId)
                .ToList();

                if (existingFlow.Count < 1)
                {
                    FlowsModel floww = new FlowsModel(flow);
                    db.Add(floww);
                    localDeviceId = flow.LocalDeviceId!;
                    Debug.WriteLine("OK Saved flow db add");
                }
                if (existingFlow?.Count > 0)
                {
                    FlowsModel dbFlow= new FlowsModel(flow);

                    db.Add(dbFlow, update: true);
                    localDeviceId = flow.LocalDeviceId!;
                    Debug.WriteLine("OK Saved flow db up");
                }
            });

            if (CurrentUserOnline is not null)
            {
                if (CurrentUserOnline.IsAuthenticated)
                {
                    _ = SendSingleObjectToParse(nameof(FlowsModelView), flow);
                    Debug.WriteLine("OK Saved flow online");
                }
            }
            return localDeviceId;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error when updating song: " + ex.Message);
            return ex.Message;
        }
    }
    public async Task LoadFlowsToDBFromOnline()
    {
        var AllItems = await ParseClient.Instance.CallCloudCodeFunctionAsync<List<object>>
            (
                "getFlowsForUser", new Dictionary<string, object>
                {
                    
                }
            );
      

        //var UniqueItems = AllItems.DistinctBy(x => x["DeviceFormFactor"]).ToList();
        if (AllItems != null && AllItems.Count != 0)
        {
            AllFlows?.Clear();
            // Get the realm database instance.
            db = Realm.GetInstance(DataBaseService.GetRealm());
            db.Write(() =>
            {
                foreach (var item in AllItems)
                {
                    try
                    {
                        FlowsModelView? itemmm = MapToModelFromParseObject<FlowsModelView>((ParseObject)item); //duration is off
                        AllFlows?.Add(itemmm);
                        FlowsModel itemm = new FlowsModel(itemmm);

                        var existingSongs = db.All<FlowsModel>()
                                                .Where(s => s.LocalDeviceId == itemmm.LocalDeviceId)
                                                .ToList();

                        if (existingSongs.Count < 1)
                        {
                            db.Add(itemm);                            
                        }
                        else
                        {
                            db.Add(itemm, update: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing artist: {ex.Message}");
                    }
                }
            });
        }
        //GetFlows();
    }
    public async Task SyncAllData()
    {
        await LoadFlowsToDBFromOnline();
        await LoadFlowCommentsToDBFromOnline();
        await LoadFlowAndCommentsLinksToDBFromOnline();
    }
public async Task LoadFlowCommentsToDBFromOnline()
    {
      
        var query = ParseClient.Instance.GetQuery("FlowCommentsView")
              .WhereEqualTo("ACL", CurrentUserOnline.ObjectId);

        var AllItems = await query.FindAsync();
     

        //var UniqueItems = AllItems.DistinctBy(x => x["DeviceFormFactor"]).ToList();
        if (AllItems != null && AllItems.Any())
        {
            AllFlows?.Clear();
            // Get the realm database instance.
            db = Realm.GetInstance(DataBaseService.GetRealm());
            db.Write(() =>
            {
                foreach (var item in AllItems)
                {
                    try
                    {
                        FlowCommentsView? itemmm = MapToModelFromParseObject<FlowCommentsView>((ParseObject)item); //duration is off
                        AllFlowComments?.Add(itemmm);
                        FlowComments itemm = new FlowComments(itemmm);

                        var existingSongs = db.All<FlowComments>()
                                                .Where(s => s.LocalDeviceId == itemmm.LocalDeviceId)
                                                .ToList();

                        if (existingSongs.Count < 1)
                        {
                            db.Add(itemm);                            
                        }
                        else
                        {
                            db.Add(itemm, update: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing artist: {ex.Message}");
                    }
                }
            });
        }
        //GetFlows();
    }
public async Task LoadFlowAndCommentsLinksToDBFromOnline()
    {
      
        var query = ParseClient.Instance.GetQuery("FlowModelAndCommentLink")
              .WhereEqualTo("ACL", CurrentUserOnline!.ObjectId);

        var AllItems = await query.FindAsync();
     

        //var UniqueItems = AllItems.DistinctBy(x => x["DeviceFormFactor"]).ToList();
        if (AllItems != null && AllItems.Any())
        {
            AllFlows?.Clear();
            // Get the realm database instance.
            db = Realm.GetInstance(DataBaseService.GetRealm());
            db.Write(() =>
            {
                foreach (var item in AllItems)
                {
                    try
                    {
                        FlowModelAndCommentLink? itemmm = MapToModelFromParseObject<FlowModelAndCommentLink>((ParseObject)item); //duration is off
                        AllFlowAndCommentsLink?.Add(itemmm);
                        
                        var existingSongs = db.All<FlowModelAndCommentLink>()
                                                .Where(s => s.LocalDeviceId == itemmm.LocalDeviceId)
                                                .ToList();

                        if (existingSongs.Count < 1)
                        {
                            db.Add(itemmm);                            
                        }
                        else
                        {
                            db.Add(itemmm, update: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing artist: {ex.Message}");
                    }
                }
            });
        }
        //GetFlows();
    }
    public static ParseObject MapToParseObject<T>(T model, string className)
    {
        var parseObject = new ParseObject(className);

        // Get the properties of the class
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            try
            {
                var value = property.GetValue(model);

                // Skip null values or Realm-specific/unsupported types
                if (value == null || IsRealmSpecificType(property.PropertyType))
                {
                    continue;
                }

                // Handle special types like DateTimeOffset
                if (property.PropertyType == typeof(DateTimeOffset))
                {
                    var val = (DateTimeOffset)value;
                    parseObject[property.Name] = val.Date;
                    continue;
                }

                // Handle string as string (required for Parse compatibility)
                if (property.PropertyType == typeof(string))
                {
                    parseObject[property.Name] = value.ToString();
                    continue;
                }

                // Add a fallback check for unsupported complex types
                if (value.GetType().Namespace?.StartsWith("Realms") == true)
                {
                    Debug.WriteLine($"Skipped unsupported Realm type: {property.Name}");
                    continue;
                }

                // For other types, directly set the value
                parseObject[property.Name] = value;
            }
            catch (Exception ex)
            {
                // Log the exception for this particular property, but continue with the next one
                Debug.WriteLine($"Error when mapping property '{property.Name}': {ex.Message}");
            }
        }

        return parseObject;
    }

    public static T MapToModelFromParseObject<T>(ParseObject parseObject) where T : new()
    {
        var model = new T();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            try
            {
                // Skip Realm-specific properties
                if (IsRealmSpecificType(property.PropertyType))
                {
                    continue;
                }

                // Check if the ParseObject contains the property name
                if (parseObject.ContainsKey(property.Name))
                {
                    var value = parseObject[property.Name];

                    if (value != null)
                    {
                        // Handle special types like DateTimeOffset
                        if (property.PropertyType == typeof(DateTimeOffset) && value is DateTime dateTime)
                        {
                            property.SetValue(model, new DateTimeOffset(dateTime));
                            continue;
                        }

                        // Handle string as string
                        if (property.PropertyType == typeof(string) && value is string objectIdStr)
                        {
                            property.SetValue(model, new string(objectIdStr));
                            continue;
                        }

                        if (property.CanWrite && property.PropertyType.IsAssignableFrom(value.GetType()))
                        {
                            property.SetValue(model, value);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // Log and skip the property
                Debug.WriteLine($"Error mapping property '{property.Name}': {ex.Message}");
            }
        }

        return model;
    }

    public static bool IsRealmSpecificType(Type type)
    {

        return type.IsSubclassOf(typeof(RealmObject)) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(RealmList<>) || type == typeof(DynamicObjectApi);
    }


    public bool SyncAllDataToDatabaseAsync(IEnumerable<FlowsModelView> flows,
            IEnumerable<FlowCommentsView>flowComments)
    {
        try
        {
            db = Realm.GetInstance(DataBaseService.GetRealm());
            // Ensure UserModel exists
            var user = db.All<UserModel>().FirstOrDefault();
            if (user == null)
            {
                db.Write(() =>
                {
                    user = new UserModel
                    {
                        // Set other properties as needed
                    };
                    db.Add(user);
                });
            }
            db.Write(() =>
            {
            AddOrUpdateMultipleRealmItems(
                flows.Select(x => new FlowsModel(x)),
                flow => db.All<FlowsModel>().Any(x => x.LocalDeviceId == flow.LocalDeviceId)
                );
            });
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error syncing data to database: {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// Maps Collection of ModelView objects to ParseObjects then Sends to Online DB
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="modelName"></param>
    /// <returns></returns>
    public async Task<bool> SendMultipleObjectsToParse<T>(IEnumerable<T> items, string modelName)
    {
        try
        {
            foreach (var item in items)
            {
                try
                {
                    // Map and save each item to Parse
                    await SendSingleObjectToParse(modelName, item);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving {modelName} action: {ex.Message}");
                }
            }
            Debug.WriteLine($"{modelName}sToOnline saved!");
            await Shell.Current.DisplayAlert("Success!", "Synced!", "Ok");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception during sync for {modelName}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Maps Single ModelView object to ParseObject then Sends to Online DB
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="modelName"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static async Task SendSingleObjectToParse<T>(string modelName, T? item)
    {
        var parseObj = MapToParseObject(item, modelName);
        await parseObj.SaveAsync();
    }
    /// <summary>
    /// Adds or updates a collection of items in the specified Realm database.
    /// </summary>
    /// <typeparam name="T">The type of the Realm object being added or updated. Must inherit from RealmObject.</typeparam>
    /// <param name="items">
    /// A collection of items of type <typeparamref name="T"/> that need to be synchronized with the Realm database.
    /// </param>
    /// <param name="existsCondition">
    /// A condition used to determine if an item already exists in the database. This is a delegate that accepts
    /// an item of type <typeparamref name="T"/> and returns a boolean value indicating whether the item exists.
    /// </param>
    /// <param name="updateAction">
    /// An optional delegate that allows additional custom updates to an item if it already exists in the database.
    /// If provided, this delegate will be invoked before the item is added or updated in the database.
    /// </param>
    /// <remarks>
    /// This method performs a batch operation to synchronize a collection of items with the Realm database. 
    /// It uses the <paramref name="existsCondition"/> to check if an item already exists in the database.
    /// - If an item does not exist, it is added to the database.
    /// - If an item exists, the <paramref name="updateAction"/> (if provided) is invoked for additional updates,
    ///   and the item is updated in the database.
    ///
    /// This method wraps all database operations within a single write transaction to ensure atomicity.
    /// </remarks>
    /// <exception cref="Exception">Thrown if there is an error during the write transaction.</exception>
    /// <example>
    /// Example usage:
    /// <code>
    /// var songs = new List&lt;SongModel&gt; { song1, song2, song3 };
    /// AddOrUpdateMultipleRealmItems(
    ///     songs,
    ///     song =&gt; db.All&lt;SongModel&gt;().Any(s =&gt; s.Title == song.Title &amp;&amp; s.ArtistName == song.ArtistName),
    ///     existingSong =>
    ///     {
    ///         existingSong.PlayCount = song.PlayCount; // Example custom update
    ///     });
    /// </code>
    /// </example>

    public void AddOrUpdateMultipleRealmItems<T>(IEnumerable<T> items, Func<T, bool> existsCondition, Action<T>? updateAction = null) where T : RealmObject
    {
        try
        {

            db = Realm.GetInstance(DataBaseService.GetRealm());
            db.Write(() =>
            {
                foreach (var item in items)
                {
                    if (!db.All<T>().Any(existsCondition))
                    {
                        db.Add(item);
                        Debug.WriteLine($"Added {typeof(T).Name}");
                    }
                    else
                    {
                        updateAction?.Invoke(item); // Perform additional updates if needed
                        db.Add(item, update: true); // Update existing item
                        Debug.WriteLine($"Updated {typeof(T).Name}");
                    }
                }
            });
        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }
    }

    public void AddOrUpdateSingleRealmItem<T>(Realm db, T item, Func<T, bool> existsCondition, Action<T>? updateAction = null) where T : RealmObject
    {

        db = Realm.GetInstance(DataBaseService.GetRealm());
        db.Write(() =>
        {
            if (!db.All<T>().Any(existsCondition))
            {
                db.Add(item);
                Debug.WriteLine($"Added {typeof(T).Name}");
            }
            else
            {
                updateAction?.Invoke(item); // Perform additional updates if needed
                db.Add(item, update: true); // Update existing item
                Debug.WriteLine($"Updated {typeof(T).Name}");
            }
        });
    }


    public void UpdateUser(UserModelView user)
    {
        db = Realm.GetInstance(DataBaseService.GetRealm());
        db.Write(() =>
        {

            db.RemoveAll<UserModel>();

            UserModel userm = new UserModel(user);
            db.Add(userm);
        });
        if (CurrentUserOnline is not null)
        {
            if (CurrentUserOnline.IsAuthenticated)
            {
                _ = SendSingleObjectToParse("_User", user);
            }
        }
    }
}
