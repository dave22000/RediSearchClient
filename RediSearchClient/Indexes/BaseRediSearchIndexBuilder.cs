using System;
using System.Collections.Generic;
using System.Linq;

namespace RediSearchClient.Indexes
{
    /// <summary>
    /// This is the builder wherein a majority of an index is defined.
    /// </summary>
    public abstract class BaseRediSearchIndexBuilder<TFieldBuilder> where TFieldBuilder : new()
    {
        internal BaseRediSearchIndexBuilder()
        {
        }

        private List<string> _prefixes;

        private bool HasPrefixes() => _prefixes?.Any() ?? false;

        /// <summary>
        /// Builder method for defining the key pattern to index.
        /// </summary>
        /// <param name="prefix">Key pattern for which items to index.</param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> ForKeysWithPrefix(string prefix)
        {
            if (_prefixes == null)
            {
                _prefixes = new List<string>(2);
            }

            _prefixes.Add(prefix);

            return this;
        }

        private string _filter;

        /// <summary>
        /// Allows for specifying a filter for indexable items.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> UsingFilter(string filter)
        {
            _filter = filter;

            return this;
        }

        private string _language;

        /// <summary>
        /// Sets the language for the index, defaults to English.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> UsingLanguage(string language)
        {
            _language = language;

            return this;
        }

        private string _languageField;

        /// <summary>
        /// Sets the field to source the document language from.
        /// </summary>
        /// <param name="languageField"></param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> UsingLanguageField(string languageField)
        {
            _languageField = languageField;

            return this;
        }

        private double _score = 1;

        /// <summary>
        /// Sets the default score for documents.
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> SetScore(double score)
        {
            _score = score;

            return this;
        }

        private string _scoreField;

        /// <summary>
        /// Sets the field to source the document score from.
        /// </summary>
        /// <param name="scoreField"></param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> SetScoreField(string scoreField)
        {
            _scoreField = scoreField;

            return this;
        }

        private string _payloadField;

        /// <summary>
        /// Sets the field that should be used as a binary safe payload string for the document.
        /// </summary>
        /// <param name="payloadField"></param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> SetPayloadField(string payloadField)
        {
            _payloadField = payloadField;

            return this;
        }

        private bool _maxTextFields;

        /// <summary>
        /// For efficiency, RediSearch encodes indexes differently if they are created with less 
        /// than 32 text fields. This option forces RediSearch to encode indexes as if there were 
        /// more than 32 text fields, which allows you to add additional fields (beyond 32) using 
        /// `AlterSchema` or `AlterSchemaAsync`.
        /// </summary>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> MaxTextFields()
        {
            _maxTextFields = true;

            return this;
        }

        private bool _noOffsets;

        /// <summary>
        /// If set, we do not store term offsets for documents (saves memory, does not allow 
        /// exact searches or highlighting).
        /// </summary>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> NoOffsets()
        {
            _noOffsets = true;

            return this;
        }

        private bool _noHighLights;

        /// <summary>
        /// Conserves storage space and memory by disabling highlighting support. If set, we 
        /// do not store corresponding byte offsets for term positions.
        /// </summary>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> NoHighLights()
        {
            _noHighLights = true;

            return this;
        }

        private int _lifespanInSeconds;

        /// <summary>
        /// Create a lightweight temporary index which will expire after the specified period 
        /// of inactivity. The internal idle timer is reset whenever the index is searched or 
        /// added to. Because such indexes are lightweight, you can create thousands of such 
        /// indexes without negative performance implications and therefore you should consider 
        /// using `.SkipInitialScan()` to avoid costly scanning.
        /// </summary>
        /// <param name="lifespanInSeconds"></param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> Temporary(int lifespanInSeconds)
        {
            _lifespanInSeconds = lifespanInSeconds;

            return this;
        }

        private bool _noFields;

        /// <summary>
        /// If set, we do not store field bits for each term. Saves memory, does not allow 
        /// filtering by specific fields.
        /// </summary>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> NoFields()
        {
            _noFields = true;

            return this;
        }

        private bool _noFrequencies;

        /// <summary>
        /// If set, we avoid saving the term frequencies in the index. This saves memory 
        /// but does not allow sorting based on the frequencies of a given term within the document.
        /// </summary>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> NoFrequencies()
        {
            _noFrequencies = true;

            return this;
        }

        private List<string> _stopwords;
        private bool _withNoStopwords = false;

        private bool OverrideDefaultStopwords() => _stopwords?.Any() ?? false || _withNoStopwords ;

        /// <summary>
        /// Builder method for setting the index with a custom stopword list,
        /// to be ignored during indexing and search time
        /// If not set, FT.CREATE takes the default list of stopwords. If {count} is set to 0, the index does not have stopwords.
        /// </summary>
        /// <param name="stopwords">A collection of stopwords which will override the defaults</param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> WithStopwords(params string[] stopwords)
        {
            if (_withNoStopwords)
            {
                throw new Exception("Conflicting Stopwords configuration");
            }
            if (_stopwords == null)
            {
                _stopwords = new List<string>(stopwords.Count());
            }

            _stopwords.AddRange(stopwords);

            return this;
        }

        /// <summary>
        /// Builder method for setting the index to use no stopwords
        /// This must be explicitly called to override the default stopwords list.
        /// </summary>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> WithNoStopwords(){
            if (_stopwords != null)
            {
                throw new Exception("Conflicting Stopwords configuration");
            }
            _withNoStopwords = true;
            return this;
        }

        private bool _skipInitialScan;

        /// <summary>
        /// If set, we do not scan and index.
        /// </summary>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> SkipInitialScan()
        {
            _skipInitialScan = true;

            return this;
        }

        private Func<TFieldBuilder, IRediSearchSchemaField>[] _fields;

        /// <summary>
        /// Allows for defining the schema of the search index. 
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> WithSchema(
            params Func<TFieldBuilder, IRediSearchSchemaField>[] fields)
        {
            _fields = fields;

            return this;
        }

        /// <summary>
        /// Allows for defining the schema of the search index. 
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public BaseRediSearchIndexBuilder<TFieldBuilder> WithSchema(IRediSearchSchemaField[] fields)
        {
            _fields = fields.Select(x =>
            {
                IRediSearchSchemaField func(TFieldBuilder y) => x;
                return (Func<TFieldBuilder, IRediSearchSchemaField>)func;
            }).ToArray();

            return this;
        }

        private static readonly TFieldBuilder _fieldBuilder = new TFieldBuilder();

        /// <summary>
        /// Builds the index definition. 
        /// </summary>
        /// <returns></returns>
        public RediSearchIndexDefinition Build()
        {
            var argumentLength = 2; // ON {structure}

            argumentLength += HasPrefixes() ? 2 + _prefixes.Count : 0; // [PREFIX {count} {prefix} [{prefix} ..]
            argumentLength += string.IsNullOrEmpty(_filter) ? 0 : 2; // [FILTER {filter}]		
            argumentLength += string.IsNullOrEmpty(_language) ? 0 : 2; // [LANGUAGE {default_lang}]
            argumentLength += string.IsNullOrEmpty(_languageField) ? 0 : 2; // [LANGUAGE_FIELD {lang_field}]
            argumentLength += _score != 1 ? 2 : 0; // [SCORE {default_score}]
            argumentLength += string.IsNullOrEmpty(_scoreField) ? 0 : 2; // [SCORE_FIELD {score_field}]
            argumentLength += string.IsNullOrEmpty(_payloadField) ? 0 : 2; // [PAYLOAD_FIELD {payload_field}]
            argumentLength += _maxTextFields ? 1 : 0; // [MAXTEXTFIELDS]
            argumentLength += _lifespanInSeconds > 0 ? 2 : 0; // [TEMPORARY {seconds}]
            argumentLength += _noOffsets ? 1 : 0; // [NOOFFSETS]
            argumentLength += _noHighLights ? 1 : 0; // [NOHL]
            argumentLength += _noFields ? 1 : 0; // [NOFIELDS]
            argumentLength += _noFrequencies ? 1 : 0; // [NOFREQS]
            argumentLength += OverrideDefaultStopwords() ? 2 + (_stopwords?.Count ?? 0) : 0; // [STOPWORDS {count} [{stopwords} ..]]
            argumentLength += _skipInitialScan ? 1 : 0; // [SKIPINITIALSCAN]

            // If there are no schema fields we should probably throw an exception eh?
            var schemaFields = _fields?.Select(x => x(_fieldBuilder)).ToList();

            if (schemaFields == default)
            {
                throw new Exception("It doesn't look like you've actually defined a schema.");
            }

            argumentLength += schemaFields.Sum(x => x.FieldArguments.Length) + 1;

            var result = new object[argumentLength];

            var currentArgumentIndex = 0;

            // ON {structure}
            result[currentArgumentIndex] = "ON";
            result[++currentArgumentIndex] = ResolveStructure();

            // [PREFIX {count} {prefix} [{prefix} ..]

            if (HasPrefixes())
            {
                result[++currentArgumentIndex] = "PREFIX";
                result[++currentArgumentIndex] = _prefixes.Count;

                foreach (var prefix in _prefixes)
                {
                    result[++currentArgumentIndex] = prefix;
                }
            }

            // [FILTER {filter}]
            if (!string.IsNullOrEmpty(_filter))
            {
                result[++currentArgumentIndex] = "FILTER";
                result[++currentArgumentIndex] = _filter;
            }

            // [LANGUAGE {default_lang}]
            if (!string.IsNullOrEmpty(_language))
            {
                result[++currentArgumentIndex] = "LANGUAGE";
                result[++currentArgumentIndex] = _language;
            }

            // [LANGUAGE_FIELD {lang_field}]
            if (!string.IsNullOrEmpty(_languageField))
            {
                result[++currentArgumentIndex] = "LANGUAGE_FIELD";
                result[++currentArgumentIndex] = _languageField;
            }

            // [SCORE {default_score}]
            if (_score != 1)
            {
                result[++currentArgumentIndex] = "SCORE";
                result[++currentArgumentIndex] = _score;
            }

            // [SCORE_FIELD {score_field}]
            if (!string.IsNullOrEmpty(_scoreField))
            {
                result[++currentArgumentIndex] = "SCORE_FIELD";
                result[++currentArgumentIndex] = _scoreField;
            }

            // [PAYLOAD_FIELD {payload_field}]
            if (!string.IsNullOrEmpty(_payloadField))
            {
                result[++currentArgumentIndex] = "PAYLOAD_FIELD";
                result[++currentArgumentIndex] = _payloadField;
            }

            // [MAXTEXTFIELDS]
            if (_maxTextFields)
            {
                result[++currentArgumentIndex] = "MAXTEXTFIELDS";
            }

            // [TEMPORARY {seconds}]
            if (_lifespanInSeconds > 0)
            {
                result[++currentArgumentIndex] = "TEMPORARY";
                result[++currentArgumentIndex] = _lifespanInSeconds;
            }

            // [NOOFFSETS]
            if (_noOffsets)
            {
                result[++currentArgumentIndex] = "NOOFFSETS";
            }

            // [NOHL]
            if (_noHighLights)
            {
                result[++currentArgumentIndex] = "NOHL";
            }

            // [NOFIELDS]
            if (_noFields)
            {
                result[++currentArgumentIndex] = "NOFIELDS";
            }

            // [NOFREQS]
            if (_noFrequencies)
            {
                result[++currentArgumentIndex] = "NOFREQS";
            }

            // [STOPWORDS {count} [{stopwords} {prefix} ..]
            if (OverrideDefaultStopwords())
            {
                result[++currentArgumentIndex] = "STOPWORDS";

                if (_withNoStopwords){ 
                    result[++currentArgumentIndex] = 0;
                } else {
                    result[++currentArgumentIndex] = _stopwords.Count;
                    foreach (var stopword in _stopwords)
                    {
                        result[++currentArgumentIndex] = stopword;
                    } 
                }
            }

            // [SKIPINITIALSCAN]
            if (_skipInitialScan)
            {
                result[++currentArgumentIndex] = "SKIPINITIALSCAN";
            }

            // SCHEMA {field} [TEXT [NOSTEM] [WEIGHT {weight}] [PHONETIC {matcher}] | NUMERIC | GEO | TAG [SEPARATOR {sep}] ] [SORTABLE][NOINDEX] ...
            result[++currentArgumentIndex] = "SCHEMA";

            foreach (var field in schemaFields.SelectMany(x => x.FieldArguments))
            {
                result[++currentArgumentIndex] = field;
            }

            // ReSharper disable once HeapView.ObjectAllocation.Evident
            return new RediSearchIndexDefinition(result);
        }

        /// <summary>
        /// This method returns a string representation of the kind of data structure that the index is being applied to.
        /// </summary>
        /// <returns></returns>
        protected abstract string ResolveStructure();
    }
}