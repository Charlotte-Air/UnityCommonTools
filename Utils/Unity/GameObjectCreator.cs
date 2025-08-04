using UnityEngine;

namespace Framework.Utils
{
  public static class GameObjectCreator
  {
    public static CreateCallback OnBeforeCreate;
    public static CreateCallback OnAfterCreate;
    public static DestroyCallback OnBeforeDestroy;

    public static T Instantiate<T>(T original, string catalogue = "no_catalogue") where T : Object
    {
      _OnBeforeCreate((Object) null, (Object) original, catalogue);
      T go = Object.Instantiate<T>(original);
      _OnAfterCreate((Object) go, (Object) original, catalogue);
      return go;
    }

    public static T Instantiate<T>(T original, Transform parent, string catalogue = "no_catalogue") where T : Object
    {
      _OnBeforeCreate((Object) null, (Object) original, catalogue);
      T go = Object.Instantiate<T>(original, parent);
      _OnAfterCreate((Object) go, (Object) original, catalogue);
      return go;
    }

    public static Object Instantiate(Object original, string catalogue = "no_catalogue")
    {
      _OnBeforeCreate((Object) null, original, catalogue);
      Object go = Object.Instantiate(original);
      _OnAfterCreate(go, original, catalogue);
      return go;
    }

    public static Object Instantiate(Object original, Transform parent, string catalogue = "no_catalogue")
    {
      _OnBeforeCreate((Object) null, original, catalogue);
      Object go = Object.Instantiate(original, parent);
      _OnAfterCreate(go, original, catalogue);
      return go;
    }

    public static Object Instantiate(
      Object original,
      Transform parent,
      bool worldPositionStays,
      string catalogue = "no_catalogue")
    {
      _OnBeforeCreate((Object) null, original, catalogue);
      Object go = Object.Instantiate(original, parent, worldPositionStays);
      _OnAfterCreate(go, original, catalogue);
      return go;
    }

    public static Object Instantiate(
      Object original,
      Vector3 position,
      Quaternion rotation,
      string catalogue = "no_catalogue")
    {
      _OnBeforeCreate((Object) null, original, catalogue);
      Object go = Object.Instantiate(original, position, rotation);
      _OnAfterCreate(go, original, catalogue);
      return go;
    }

    public static Object Instantiate(
      Object original,
      Vector3 position,
      Quaternion rotation,
      Transform parent,
      string catalogue = "no_catalogue")
    {
      _OnBeforeCreate((Object) null, original, catalogue);
      Object go = Object.Instantiate(original, position, rotation, parent);
      _OnAfterCreate(go, original, catalogue);
      return go;
    }

    public static GameObject New(string name = null, string catalogue = "no_catalogue")
    {
      _OnBeforeCreate((Object) null, (Object) null, catalogue);
      GameObject go = new GameObject(name ?? string.Empty);
      _OnAfterCreate((Object) go, (Object) null, catalogue);
      return go;
    }

    public static void Destroy(Object o)
    {
      if (!(o != (Object) null))
        return;
      _OnBeforeDestroy(o);
      if (Application.isPlaying)
        Object.Destroy(o);
      else
        Object.DestroyImmediate(o);
    }

    private static void _OnBeforeCreate(Object go, Object original, string catalogue)
    {
      if (OnBeforeCreate == null)
        return;
      OnBeforeCreate(go, original, catalogue);
    }

    private static void _OnAfterCreate(Object go, Object original, string catalogue)
    {
      if (OnAfterCreate == null)
        return;
      OnAfterCreate(go, original, catalogue);
    }

    private static void _OnBeforeDestroy(Object go)
    {
      if (OnBeforeDestroy == null)
        return;
      OnBeforeDestroy(go);
    }

    public delegate void CreateCallback(Object go, Object original, string catalogue);

    public delegate void DestroyCallback(Object go);
  }
}