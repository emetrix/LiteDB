﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LiteDB
{
    public partial class LiteCollection<T>
    {
        /// <summary>
        /// Update a document in this collection. Returns false if not found document in collection
        /// </summary>
        public bool Update(T document)
        {
            if (document == null) throw new ArgumentNullException("document");

            // get BsonDocument from object
            var doc = _mapper.ToDocument(document);

            var id = doc["_id"];

            if (id.IsNull || id.IsMinValue || id.IsMaxValue) throw LiteException.InvalidDataType("_id", id);

            return _engine.UpdateDocument(_name, id, doc);
        }

        /// <summary>
        /// Update a document in this collection. Returns false if not found document in collection
        /// </summary>
        public bool Update(BsonValue id, T document)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (id == null || id.IsNull) throw new ArgumentNullException("id");

            // get BsonDocument from object
            var doc = _mapper.ToDocument(document);

            return _engine.UpdateDocument(_name, id, doc);
        }

        /// <summary>
        /// Query documents and execute, for each document, action method. After action, update each document
        /// </summary>
        public int Update(Query query, Action<T> action)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (action == null) throw new ArgumentNullException("action");

            var docs = this.Find(query).ToArray(); // used to avoid changes during Action<T>
            var count = 0;

            foreach (var doc in docs)
            {
                action(doc);

                // get BsonDocument from object
                var bson = _mapper.ToDocument(doc);

                var id = bson["_id"];

                if (id.IsNull || id.IsMinValue || id.IsMaxValue) throw LiteException.InvalidDataType("_id", id);

                count += _engine.UpdateDocument(_name, id, bson) ? 1 : 0;
            }

            return count;
        }

        /// <summary>
        /// Query documents and execute, for each document, action method. All data is locked during execution
        /// </summary>
        public void Update(Expression<Func<T, bool>> predicate, Action<T> action)
        {
            this.Update(_visitor.Visit(predicate), action);
        }
    }
}
