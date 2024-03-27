using Beamable.Common;
using MongoDB.Driver;

namespace Beamable.Server
{
	[StorageObject("ThetaStorage")]
	public class ThetaStorage : MongoStorageObject
	{
	}

	public static class ThetaStorageExtension
	{
		/// <summary>
		/// Get an authenticated MongoDB instance for ThetaStorage
		/// </summary>
		/// <returns></returns>
		public static Promise<IMongoDatabase> ThetaStorageDatabase(this IStorageObjectConnectionProvider provider)
			=> provider.GetDatabase<ThetaStorage>();

		/// <summary>
		/// Gets a MongoDB collection from ThetaStorage by the requested name, and uses the given mapping class.
		/// If you don't want to pass in a name, consider using <see cref="ThetaStorageCollection{TCollection}()"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> ThetaStorageCollection<TCollection>(
			this IStorageObjectConnectionProvider provider, string name)
			where TCollection : StorageDocument
			=> provider.GetCollection<ThetaStorage, TCollection>(name);

		/// <summary>
		/// Gets a MongoDB collection from ThetaStorage by the requested name, and uses the given mapping class.
		/// If you want to control the collection name separate from the class name, consider using <see cref="ThetaStorageCollection{TCollection}(string)"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> ThetaStorageCollection<TCollection>(
			this IStorageObjectConnectionProvider provider)
			where TCollection : StorageDocument
			=> provider.GetCollection<ThetaStorage, TCollection>();
	}
}
