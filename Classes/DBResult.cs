namespace QDMS.Classes
{
    public class DBResult<T> where T : class
    {
        public bool IsSuccessful { get; private set; } = false;
        public bool IsError { get; private set; } = false;
        public T Value { get; private set; }
        public Exception? Exception { get; private set; }

        public static readonly DBResult<T> Failed = new DBResult<T>();
        public static DBResult<T> Create(T value) => new DBResult<T>(value!);
        public static DBResult<T> CreateIfNotNull(T? value) => value == null ? Failed : new DBResult<T>(value!);
        public static DBResult<T> CreateIf(bool check, T value) => !check ? Failed : new DBResult<T>(value);
        public static DBResult<T> CreateFailed(Exception ex) => new DBResult<T>(ex);
        public static DBResult<T> CreateFailed() => new DBResult<T>();

        //public static DBResult<IEnumerable<A>> CreateIfNotNullOrEmpty<A>(IEnumerable<A> e)
        //        => e == null || e.Count() == 0 ? DBResult<IEnumerable<A>>.Failed : new DBResult<IEnumerable<A>>(e!);

        public DBResult(T value)
        {
            IsSuccessful = true;
            IsError = false;
            Value = value;
            Exception = null;
        }

        public DBResult(Exception ex)
        {
            IsSuccessful = false;
            IsError = true;
            Value = null;
            Exception = ex;
        }

        public DBResult()
        {
            IsSuccessful = false;
            IsError = false;
            Value = null;
            Exception = null;
        }

        public bool TryGetValue(out T value)
        {
            value = null;

            if (this.IsSuccessful)
            {
                value = this.Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        public DBResult<TTo> To<TTo>(Func<T, TTo?> func) where TTo : class
        {
            if (this.IsSuccessful && !this.IsError)
                return DBResult<TTo>.CreateIfNotNull(func(this.Value));

            if (this.IsError)
                return DBResult<TTo>.CreateFailed(this.Exception!);
            else
                return DBResult<TTo>.CreateFailed();
        }
    }

    public class DBResult
    {
        public bool IsSuccessful { get; private set; } = false;
        public bool IsError { get; private set; } = false;
        public Exception? Exception { get; private set; }

        public static DBResult Create(bool isSuccess = true) => new DBResult(isSuccess);
        public static DBResult CreateFailed(Exception ex) => new DBResult(ex);
        public static DBResult CreateFailed() => new DBResult(false);

        public DBResult(bool isSuccess)
        {
            IsSuccessful = isSuccess;
            IsError = false;
            Exception = null;
        }

        public DBResult(Exception ex)
        {
            IsSuccessful = false;
            IsError = true;
            Exception = ex;
        }


        private DBResult()
        {

        }
    }
}
