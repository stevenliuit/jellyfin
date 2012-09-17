﻿using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Controller.Entities
{
    public abstract class BaseItem : BaseEntity, IHasProviderIds
    {
        protected ItemResolveEventArgs _resolveArgs;
        /// <summary>
        /// We attach these to the item so that we only ever have to hit the file system once
        /// (this includes the children of the containing folder)
        /// Use ResolveArgs.FileSystemChildren to check for the existence of files instead of File.Exists
        /// </summary>
        public ItemResolveEventArgs ResolveArgs
        {
            get
            {
                if (_resolveArgs == null)
                {
                    _resolveArgs = new ItemResolveEventArgs()
                    {
                        FileInfo = FileData.GetFileData(this.Path),
                        Parent = this.Parent,
                        Cancel = false,
                        Path = this.Path
                    };
                    _resolveArgs = FileSystemHelper.FilterChildFileSystemEntries(_resolveArgs, (this.Parent != null && this.Parent.IsRoot));
                }
                return _resolveArgs;
            }
            set
            {
                _resolveArgs = value;
            }
        }

        public string SortName { get; set; }

        /// <summary>
        /// When the item first debuted. For movies this could be premiere date, episodes would be first aired
        /// </summary>
        public DateTime? PremiereDate { get; set; }

        public string Path { get; set; }

        public Folder Parent { get; set; }

        public string LogoImagePath { get; set; }

        public string ArtImagePath { get; set; }

        public string ThumbnailImagePath { get; set; }

        public string BannerImagePath { get; set; }

        public IEnumerable<string> BackdropImagePaths { get; set; }

        public string OfficialRating { get; set; }
        
        public string CustomRating { get; set; }
        public string CustomPin { get; set; }

        public string Language { get; set; }
        public string Overview { get; set; }
        public List<string> Taglines { get; set; }

        /// <summary>
        /// Using a Dictionary to prevent duplicates
        /// </summary>
        public Dictionary<string,PersonInfo> People { get; set; }

        public List<string> Studios { get; set; }

        public List<string> Genres { get; set; }

        public string DisplayMediaType { get; set; }

        public float? UserRating { get; set; }
        public long? RunTimeTicks { get; set; }

        public string AspectRatio { get; set; }
        public int? ProductionYear { get; set; }

        /// <summary>
        /// If the item is part of a series, this is it's number in the series.
        /// This could be episode number, album track number, etc.
        /// </summary>
        public int? IndexNumber { get; set; }

        /// <summary>
        /// For an episode this could be the season number, or for a song this could be the disc number.
        /// </summary>
        public int? ParentIndexNumber { get; set; }

        public IEnumerable<Video> LocalTrailers { get; set; }

        public string TrailerUrl { get; set; }

        public Dictionary<string, string> ProviderIds { get; set; }

        public Dictionary<Guid, UserItemData> UserData { get; set; }

        public UserItemData GetUserData(User user, bool createIfNull)
        {
            if (UserData == null || !UserData.ContainsKey(user.Id))
            {
                if (createIfNull)
                {
                    AddUserData(user, new UserItemData());
                }
                else
                {
                    return null;
                }
            }

            return UserData[user.Id];
        }

        private void AddUserData(User user, UserItemData data)
        {
            if (UserData == null)
            {
                UserData = new Dictionary<Guid, UserItemData>();
            }

            UserData[user.Id] = data;
        }

        /// <summary>
        /// Determines if a given user has access to this item
        /// </summary>
        internal bool IsParentalAllowed(User user)
        {
            return true;
        }

        /// <summary>
        /// Finds an item by ID, recursively
        /// </summary>
        public virtual BaseItem FindItemById(Guid id)
        {
            if (Id == id)
            {
                return this;
            }

            if (LocalTrailers != null)
            {
                return LocalTrailers.FirstOrDefault(i => i.Id == id);
            }

            return null;
        }

        public virtual bool IsFolder
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Determine if we have changed vs the passed in copy
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public virtual bool IsChanged(BaseItem original)
        {
            bool changed = original.DateModified != this.DateModified;
            changed |= original.DateCreated != this.DateCreated;
            return changed;
        }

        /// <summary>
        /// Refresh metadata on us by execution our provider chain
        /// </summary>
        /// <returns>true if a provider reports we changed</returns>
        public bool RefreshMetadata()
        {
            return false;
        }

        /// <summary>
        /// Determines if the item is considered new based on user settings
        /// </summary>
        public bool IsRecentlyAdded(User user)
        {
            return (DateTime.UtcNow - DateCreated).TotalDays < user.RecentItemDays;
        }

        public void AddPerson(PersonInfo person)
        {
            if (People == null)
            {
                People = new Dictionary<string, PersonInfo>(StringComparer.OrdinalIgnoreCase);
            }

            People[person.Name] = person;
        }

        /// <summary>
        /// Marks the item as either played or unplayed
        /// </summary>
        public virtual void SetPlayedStatus(User user, bool wasPlayed)
        {
            UserItemData data = GetUserData(user, true);

            if (wasPlayed)
            {
                data.PlayCount = Math.Max(data.PlayCount, 1);
            }
            else
            {
                data.PlayCount = 0;
                data.PlaybackPositionTicks = 0;
            }
        }
    }
}
