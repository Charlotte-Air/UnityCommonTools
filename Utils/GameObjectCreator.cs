using UnityEngine;

namespace Charlotte.Client.Framework
{
  public static class GameObjectCreator
  {
    public static GameObjectCreator.CreateCallback OnBeforeCreate;
    public static GameObjectCreator.CreateCallback OnAfterCreate;
    public static GameObjectCreator.DestroyCallback OnBeforeDestroy;

    public static T Instantiate<T>(T original, string catalogue = "no_catalogue") where T : Object
    {
      GameObjectCreator._OnBeforeCreate((Object) null, (Object) original, catalogue);
      T go = Object.Instantiate<T>(original);
      GameObjectCreator._OnAfterCreate((Object) go, (Object) original, catalogue);
      return go;
    }

    public static T Instantiate<T>(T original, Transform parent, string catalogue = "no_catalogue") where T : Object
    {
      GameObjectCreator._OnBeforeCreate((Object) null, (Object) original, catalogue);
      T go = Object.Instantiate<T>(original, parent);
      GameObjectCreator._OnAfterCreate((Object) go, (Object) original, catalogue);
      return go;
    }

    public static Object Instantiate(Object original, string catalogue = "no_catalogue")
    {
      GameObjectCreator._OnBeforeCreate((Object) null, original, catalogue);
      Object go = Object.Instantiate(original);
      GameObjectCreator._OnAfterCreate(go, original, catalogue);
      return go;
    }

    public static Object Instantiate(Object original, Transform parent, string catalogue = "no_catalogue")
    {
      GameObjectCreator._OnBeforeCreate((Object) null, original, catalogue);
      Object go = Object.Instantiate(original, parent);
      GameObjectCreator._OnAfterCreate(go, original, catalogue);
      return go;
    }

    public static Object Instantiate(
      Object original,
      Transform parent,
      bool worldPositionStays,
      string catalogue = "no_catalogue")
    {
      GameObjectCreator._OnBeforeCreate((Object) null, original, catalogue);
      Object go = Object.Instantiate(original, parent, worldPositionStays);
      GameObjectCreator._OnAfterCreate(go, original, catalogue);
      return go;
    }

    public static Object Instantiate(
      Object original,
      Vector3 position,
      Quaternion rotation,
      string catalogue = "no_catalogue")
    {
      GameObjectCreator._OnBeforeCreate((Object) null, original, catalogue);
      Object go = Object.Instantiate(original, position, rotation);
      GameObjectCreator._OnAfterCreate(go, original, catalogue);
      return go;
    }

    public static Object Instantiate(
      Object original,
      Vector3 position,
      Quaternion rotation,
      Transform parent,
      string catalogue = "no_catalogue")
    {
      GameObjectCreator._OnBeforeCreate((Object) null, original, catalogue);
      Object go = Object.Instantiate(original, position, rotation, parent);
      GameObjectCreator._OnAfterCreate(go, original, catalogue);
      return go;
    }

    public static GameObject New(string name = null, string catalogue = "no_catalogue")
    {
      GameObjectCreator._OnBeforeCreate((Object) null, (Object) null, catalogue);
      GameObject go = new GameObject(name ?? string.Empty);
      GameObjectCreator._OnAfterCreate((Object) go, (Object) null, catalogue);
      return go;
    }

    public static void Destroy(Object o)
    {
      if (!(o != (Object) null))
        return;
      GameObjectCreator._OnBeforeDestroy(o);
      if (Application.isPlaying)
        Object.Destroy(o);
      else
        Object.DestroyImmediate(o);
    }

    private static void _OnBeforeCreate(Object go, Object original, string catalogue)
    {
      if (GameObjectCreator.OnBeforeCreate == null)
        return;
      GameObjectCreator.OnBeforeCreate(go, original, catalogue);
    }

    private static void _OnAfterCreate(Object go, Object original, string catalogue)
    {
      if (GameObjectCreator.OnAfterCreate == null)
        return;
      GameObjectCreator.OnAfterCreate(go, original, catalogue);
    }

    private static void _OnBeforeDestroy(Object go)
    {
      if (GameObjectCreator.OnBeforeDestroy == null)
        return;
      GameObjectCreator.OnBeforeDestroy(go);
    }

    public delegate void CreateCallback(Object go, Object original, string catalogue);

    public delegate void DestroyCallback(Object go);
  }
}