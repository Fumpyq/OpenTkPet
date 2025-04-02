using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Architecture
{
    public interface IAsset
    {
        Guid AssetId { get; }
        ushort TypeId { get; }
        ushort version { get; }
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }

    public class AssetSerializer
    {
        private readonly Dictionary<string, Func<IAsset>> typeFactories = new();
        private readonly Dictionary<Guid, IAsset> assetCache = new();

        public void RegisterType<T>(string typeId) where T : IAsset, new()
        {
            typeFactories[typeId] = () => new T();
        }

        public void SerializeBundle(string path, IEnumerable<IAsset> assets, bool compress = true)
        {
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(stream);

            // Header
            writer.Write("ASSET_BUNDLE"); // Magic
            writer.Write(1); // Version
            writer.Write(compress);

            // Temporary buffer for asset data
            using var assetStream = new MemoryStream();
            using var assetWriter = new BinaryWriter(assetStream);

            var assetPositions = new List<(Guid, long, int)>();

            // Write assets to temporary buffer
            foreach (var asset in assets)
            {
                assetPositions.Add((asset.AssetId, assetStream.Position, 0));
                assetWriter.Write(asset.TypeId);
                assetWriter.Write(asset.version);
                asset.Serialize(assetWriter);
                var size = (int)(assetStream.Position - assetPositions[^1].Item2);
                assetPositions[^1] = (assetPositions[^1].Item1, assetPositions[^1].Item2, size);
            }

            // Write manifest
            writer.Write(assetPositions.Count);
            foreach (var (id, offset, size) in assetPositions)
            {
                writer.Write(id.ToByteArray());
                writer.Write(offset);
                writer.Write(size);
            }

            // Write compressed asset data
            assetStream.Position = 0;
            if (compress)
            {
                using var compressor = new GZipStream(stream, CompressionMode.Compress, true);
                assetStream.CopyTo(compressor);
            }
            else
            {
                assetStream.CopyTo(stream);
            }
        }

        public IEnumerable<IAsset> DeserializeBundle(string path)
        {
            using var stream = new FileStream(path, FileMode.Open);
            using var reader = new BinaryReader(stream);

            // Validate header
            if (reader.ReadString() != "ASSET_BUNDLE")
                throw new InvalidDataException("Not a valid asset bundle");
            var version = reader.ReadInt32();
            var compressed = reader.ReadBoolean();

            // Read manifest
            var assetCount = reader.ReadInt32();
            var manifest = new List<(Guid, long, int)>(assetCount);
            for (int i = 0; i < assetCount; i++)
            {
                var id = new Guid(reader.ReadBytes(16));
                var offset = reader.ReadInt64();
                var size = reader.ReadInt32();
                manifest.Add((id, offset, size));
            }

            // Read asset data
            var assetDataStream = compressed
                ? new GZipStream(stream, CompressionMode.Decompress, true)
                : (Stream)stream;

            using var assetReader = new BinaryReader(assetDataStream);
            foreach (var (id, offset, size) in manifest)
            {
                assetDataStream.Seek(offset, SeekOrigin.Begin);
                var typeId = assetReader.ReadString();

                if (!typeFactories.TryGetValue(typeId, out var factory))
                    throw new InvalidOperationException($"Unknown asset type: {typeId}");

                var asset = factory();
                asset.Deserialize(assetReader);
                assetCache[id] = asset;
                yield return asset;
            }
        }
    }
}
