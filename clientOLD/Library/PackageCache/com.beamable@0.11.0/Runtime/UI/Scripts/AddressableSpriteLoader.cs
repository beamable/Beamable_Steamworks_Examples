using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Beamable.UI.Scripts
{
   /// <summary>
   /// Helper class for loading sprites and sprite textures from Addressable
   /// Assets.
   /// </summary>
   public static class AddressableSpriteLoader
   {
      /// <summary>
      /// Given a sprite asset reference, load the sprite itself.
      /// </summary>
      /// <param name="reference">The addressable sprite.</param>
      /// <returns>The sprite that was loaded.</returns>
      public static async Task<Sprite> LoadSprite(this AssetReferenceSprite reference)
      {
         // OperationHandle will be only be valid after the first load, but if
         // it is valid we MUST use it instead of LoadAssetAsync.

         if (!reference.RuntimeKeyIsValid())
         {
            return null; // there is no asset.
         }

         var handle = reference.OperationHandle.IsValid()
            ? reference.OperationHandle.Convert<Sprite>()
            : reference.LoadAssetAsync();
         return await SpriteFromHandle(handle);
      }

      /// <summary>
      /// Given the string path to an addressable sprite, load the sprite itself.
      /// </summary>
      /// <param name="address">Addressable path to the sprite.</param>
      /// <returns>The sprite that was loaded.</returns>
      public static async Task<Sprite> LoadSprite(string address)
      {
         return await SpriteFromHandle(Addressables.LoadAssetAsync<Sprite>(address));
      }

      /// <summary>
      /// Given an AsyncOperationHandle from an Addressable Assets loading
      /// operation, get the sprite that has been loaded or will be loaded.
      /// </summary>
      /// <param name="handle">The asynchronous operation handle.</param>
      /// <returns>The sprite, once loaded.</returns>
      public static async Task<Sprite> SpriteFromHandle(AsyncOperationHandle<Sprite> handle)
      {
         if (handle.Status == AsyncOperationStatus.Succeeded)
         {
            return handle.Result;
         }
         return await handle.Task;
      }

      /// <summary>
      /// Given a sprite asset reference, fetch its texture.
      /// </summary>
      /// <param name="reference">The addressable sprite.</param>
      /// <returns>The 2D texture of that sprite.</returns>
      public static async Task<Texture2D> LoadTexture(this AssetReferenceSprite reference)
      {
         var sprite = await LoadSprite(reference);
         return sprite == null ? null : sprite.texture;
      }
   }
}
