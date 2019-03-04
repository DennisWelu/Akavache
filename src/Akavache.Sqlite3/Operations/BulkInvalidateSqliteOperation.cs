// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using Akavache.Sqlite3.Internal;
using SQLitePCL;

namespace Akavache.Sqlite3
{
    internal class BulkInvalidateSqliteOperation : IPreparedSqliteOperation
    {
        private readonly sqlite3_stmt[] _deleteOps;
        private IDisposable _inner;

        public BulkInvalidateSqliteOperation(SQLiteConnection conn, bool useTypeInsteadOfKey)
        {
            var qs = new StringBuilder("?");
            Connection = conn;

            var column = useTypeInsteadOfKey ? "TypeName" : "Key";
            _deleteOps = Enumerable.Range(1, Constants.OperationQueueChunkSize)
                .Select(x =>
                {
                    sqlite3_stmt stmt;
                    var result = (SQLite3.Result)raw.sqlite3_prepare_v2(
                        conn.Handle,
                        $"DELETE FROM CacheElement WHERE {column} In ({qs})",
                        out stmt);

                    if (result != SQLite3.Result.OK)
                    {
                        throw new SQLiteException(result, "Couldn't prepare statement");
                    }

                    qs.Append(",?");
                    return stmt;
                })
                .ToArray();

            _inner = new CompositeDisposable(_deleteOps);
        }

        public SQLiteConnection Connection { get; protected set; }

        public Action PrepareToExecute(IEnumerable<string> toDelete)
        {
            var deleteList = toDelete.ToList();
            if (deleteList.Count == 0)
            {
                return () => { };
            }

            var deleteOp = _deleteOps[deleteList.Count - 1];
            return () =>
            {
                try
                {
                    for (int i = 0; i < deleteList.Count; i++)
                    {
                        this.Checked(raw.sqlite3_bind_text(deleteOp, i + 1, deleteList[i]));
                    }

                    this.Checked(raw.sqlite3_step(deleteOp));
                }
                finally
                {
                    this.Checked(raw.sqlite3_reset(deleteOp));
                }
            };
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _inner, Disposable.Empty).Dispose();
        }
    }
}