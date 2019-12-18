﻿// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using System.Reactive.Concurrency;

using Akavache.Sqlite3;

namespace Akavache.Tests
{
    /// <summary>
    /// Tests for the <see cref="SqlRawPersistentBlobCache"/>.
    /// </summary>
    public class SqliteBlobCacheInterfaceTests : BlobCacheInterfaceTestBase
    {
        /// <inheritdoc/>
        protected override IBlobCache CreateBlobCache(string path)
        {
            return new SqlRawPersistentBlobCache(Path.Combine(path, "sqlite.db"));
        }
    }
}
