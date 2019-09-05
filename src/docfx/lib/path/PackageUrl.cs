// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Newtonsoft.Json;

namespace Microsoft.Docs.Build
{
    /// <summary>
    /// Define the location of a package.
    /// A package is any of the following:
    ///   a) A folder
    ///   b) A git url in the form of `{remote_url}#{commit-ish}`, when cloned, results in a)
    /// The commit-sh can be any tag, sha, or branch. The default commit-ish is master.
    /// </summary>
    [JsonConverter(typeof(ShortHandConverter))]
    internal readonly struct PackageUrl
    {
        public readonly PackageType Type;

        public readonly string Path;

        public readonly string Remote;

        public readonly string Branch;

        public PackageUrl(string url)
            : this()
        {
            if (UrlUtility.IsHttp(url))
            {
                Type = PackageType.Git;
                (Remote, Branch) = SplitGitUrl(url);
            }
            else
            {
                Type = PackageType.Folder;
                Path = url;
            }
        }

        public PackageUrl(string remote, string branch)
        {
            Debug.Assert(remote != null);
            Debug.Assert(branch != null);

            Type = PackageType.Git;
            Remote = remote;
            Branch = branch;
            Path = null;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case PackageType.Folder:
                    return Path;

                case PackageType.Git:
                    return $"{Remote}#{Branch}";

                default:
                    return Type.ToString();
            }
        }

        private static (string remote, string refspec) SplitGitUrl(string remoteUrl)
        {
            Debug.Assert(!string.IsNullOrEmpty(remoteUrl));

            var (path, _, fragment) = UrlUtility.SplitUrl(remoteUrl);

            path = path.TrimEnd('/', '\\');
            var hasRefSpec = !string.IsNullOrEmpty(fragment) && fragment.Length > 1;
            var refspec = hasRefSpec ? fragment.Substring(1) : "master";

            return (path, refspec);
        }
    }
}