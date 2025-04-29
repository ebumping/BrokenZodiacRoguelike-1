using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodexOfTheBrokenZodiac.Resources;

namespace CodexOfTheBrokenZodiac.Core
{
    public partial class ResourceManager : Node
    {
        // Singleton instance
        public static ResourceManager Instance { get; private set; }

        // Signal when resources are loaded
        [Signal]
        public delegate void ResourcesLoadedEventHandler();

        // Resource caches
        private Dictionary<string, Resource> _cachedResources = new Dictionary<string, Resource>();
        private Dictionary<string, PlayerClass> _playerClasses = new Dictionary<string, PlayerClass>();
        private Dictionary<string, ZodiacSignil> _zodiacSignils = new Dictionary<string, ZodiacSignil>();
        private Dictionary<string, TarotCard> _tarotCards = new Dictionary<string, TarotCard>();
        private Dictionary<string, Weapon> _weapons = new Dictionary<string, Weapon>();
        private Dictionary<string, PackedScene> _roomTemplates = new Dictionary<string, PackedScene>();
        private Dictionary<string, Enemy> _enemyTypes = new Dictionary<string, Enemy>();

        // Resource paths
        private static readonly string CLASSES_PATH = "res://Data/Classes/";
        private static readonly string ZODIAC_PATH = "res://Data/Zodiac/";
        private static readonly string TAROT_PATH = "res://Data/Tarot/";
        private static readonly string WEAPONS_PATH = "res://Data/Weapons/";
        private static readonly string ROOMS_PATH = "res://Scenes/Rooms/";
        private static readonly string ENEMIES_PATH = "res://Data/Enemies/";

        // Loading state
        private bool _isLoaded = false;
        private int _resourcesLoading = 0;
        private int _resourcesTotal = 0;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        public override void _Ready()
        {
            GD.Print("ResourceManager initializing...");
            LoadAllResources();
        }

        private void LoadAllResources()
        {
            // Start loading all resource types
            LoadResourceDirectory<PlayerClass>(CLASSES_PATH, _playerClasses);
            LoadResourceDirectory<ZodiacSignil>(ZODIAC_PATH, _zodiacSignils);
            LoadResourceDirectory<TarotCard>(TAROT_PATH, _tarotCards, true);
            LoadResourceDirectory<Weapon>(WEAPONS_PATH, _weapons);
            LoadSceneDirectory(ROOMS_PATH, _roomTemplates);
            LoadResourceDirectory<Enemy>(ENEMIES_PATH, _enemyTypes);

            // For now, manually load the detective class and marksman rifle
            LoadSpecificResources();

            // Check if we actually started any async loads
            if (_resourcesLoading == 0)
            {
                _isLoaded = true;
                EmitSignal(SignalName.ResourcesLoaded);
            }
        }

        private void LoadSpecificResources()
        {
            // Add the Occult Detective class manually since it's required
            if (!_playerClasses.ContainsKey("OccultDetective"))
            {
                LoadResource<PlayerClass>("res://Data/Classes/OccultDetective.tres", (resource) =>
                {
                    _playerClasses["OccultDetective"] = resource;
                });
            }

            // Add the marksman rifle
            if (!_weapons.ContainsKey("MarksmanRifle"))
            {
                LoadResource<Weapon>("res://Data/Weapons/MarksmanRifle.tres", (resource) =>
                {
                    _weapons["MarksmanRifle"] = resource;
                });
            }

            // Add Aries and Taurus sigils
            if (!_zodiacSignils.ContainsKey("Aries"))
            {
                LoadResource<ZodiacSignil>("res://Data/Zodiac/AriesSignil.tres", (resource) =>
                {
                    _zodiacSignils["Aries"] = resource;
                });
            }

            if (!_zodiacSignils.ContainsKey("Taurus"))
            {
                LoadResource<ZodiacSignil>("res://Data/Zodiac/TaurusSignil.tres", (resource) =>
                {
                    _zodiacSignils["Taurus"] = resource;
                });
            }
        }

        private void LoadResourceDirectory<T>(string path, Dictionary<string, T> cache, bool recursive = false) where T : Resource
        {
            DirAccess dir = DirAccess.Open(path);
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();

                while (!string.IsNullOrEmpty(fileName))
                {
                    if (fileName == "." || fileName == "..")
                    {
                        fileName = dir.GetNext();
                        continue;
                    }

                    string fullPath = path + fileName;

                    if (dir.CurrentIsDir() && recursive)
                    {
                        LoadResourceDirectory<T>(fullPath + "/", cache, recursive);
                    }
                    else if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
                    {
                        string resourceKey = fileName.Split('.')[0];
                        LoadResource<T>(fullPath, (resource) =>
                        {
                            cache[resourceKey] = resource;
                        });
                    }

                    fileName = dir.GetNext();
                }
                dir.ListDirEnd();
            }
            else
            {
                GD.PrintErr($"Could not access directory: {path}");
            }
        }

        private void LoadSceneDirectory(string path, Dictionary<string, PackedScene> cache)
        {
            DirAccess dir = DirAccess.Open(path);
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();

                while (!string.IsNullOrEmpty(fileName))
                {
                    if (fileName == "." || fileName == "..")
                    {
                        fileName = dir.GetNext();
                        continue;
                    }

                    string fullPath = path + fileName;

                    if (dir.CurrentIsDir())
                    {
                        LoadSceneDirectory(fullPath + "/", cache);
                    }
                    else if (fileName.EndsWith(".tscn"))
                    {
                        string resourceKey = fileName.Split('.')[0];
                        LoadResource<PackedScene>(fullPath, (resource) =>
                        {
                            cache[resourceKey] = resource;
                        });
                    }

                    fileName = dir.GetNext();
                }
                dir.ListDirEnd();
            }
            else
            {
                GD.PrintErr($"Could not access directory: {path}");
            }
        }

        private void LoadResource<T>(string path, Action<T> callback) where T : Resource
        {
            if (_cachedResources.ContainsKey(path))
            {
                callback?.Invoke((T)_cachedResources[path]);
                return;
            }

            _resourcesLoading++;
            _resourcesTotal++;

            Task.Run(() =>
            {
                // This just simulates async loading, in a real implementation
                // you might use Godot's ResourceLoader.LoadThreadedRequest
                CallDeferred(MethodName.LoadResourceDeferred, path, callback);
            });
        }

        private void LoadResourceDeferred<T>(string path, Action<T> callback) where T : Resource
        {
            if (ResourceLoader.Exists(path))
            {
                Resource resource = ResourceLoader.Load(path);
                if (resource != null)
                {
                    _cachedResources[path] = resource;
                    callback?.Invoke((T)resource);
                }
                else
                {
                    GD.PrintErr($"Failed to load resource: {path}");
                }
            }
            else
            {
                GD.PrintErr($"Resource doesn't exist: {path}");
            }

            _resourcesLoading--;
            if (_resourcesLoading == 0 && !_isLoaded)
            {
                _isLoaded = true;
                EmitSignal(SignalName.ResourcesLoaded);
                GD.Print($"All resources loaded: {_resourcesTotal} total resources");
            }
        }

        // Public accessor methods
        public PlayerClass GetPlayerClass(string className)
        {
            if (_playerClasses.ContainsKey(className))
            {
                return _playerClasses[className];
            }
            GD.PrintErr($"Player class not found: {className}");
            return null;
        }

        public ZodiacSignil GetZodiacSignil(string signilName)
        {
            if (_zodiacSignils.ContainsKey(signilName))
            {
                return _zodiacSignils[signilName];
            }
            GD.PrintErr($"Zodiac sigil not found: {signilName}");
            return null;
        }

        public TarotCard GetTarotCard(string cardName)
        {
            if (_tarotCards.ContainsKey(cardName))
            {
                return _tarotCards[cardName];
            }
            GD.PrintErr($"Tarot card not found: {cardName}");
            return null;
        }

        public Weapon GetWeapon(string weaponName)
        {
            if (_weapons.ContainsKey(weaponName))
            {
                return _weapons[weaponName];
            }
            GD.PrintErr($"Weapon not found: {weaponName}");
            return null;
        }

        public PackedScene GetRoomTemplate(string templateName)
        {
            if (_roomTemplates.ContainsKey(templateName))
            {
                return _roomTemplates[templateName];
            }
            GD.PrintErr($"Room template not found: {templateName}");
            return null;
        }

        public Enemy GetEnemyType(string enemyName)
        {
            if (_enemyTypes.ContainsKey(enemyName))
            {
                return _enemyTypes[enemyName];
            }
            GD.PrintErr($"Enemy type not found: {enemyName}");
            return null;
        }

        public List<string> GetAllPlayerClasses()
        {
            return new List<string>(_playerClasses.Keys);
        }

        public List<string> GetAllZodiacSignils()
        {
            return new List<string>(_zodiacSignils.Keys);
        }

        public List<string> GetAllTarotCards()
        {
            return new List<string>(_tarotCards.Keys);
        }

        public Resource GetGenericResource(string path)
        {
            if (_cachedResources.ContainsKey(path))
            {
                return _cachedResources[path];
            }

            if (ResourceLoader.Exists(path))
            {
                Resource resource = ResourceLoader.Load(path);
                if (resource != null)
                {
                    _cachedResources[path] = resource;
                    return resource;
                }
            }

            GD.PrintErr($"Generic resource not found: {path}");
            return null;
        }
    }
}
