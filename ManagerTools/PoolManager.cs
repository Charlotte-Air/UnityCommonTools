using System.Text;
using UnityEngine;
using System.Collections.Generic;

public class PoolManager
{
	private static PoolManager _instance;
	public static PoolManager Instance => _instance ?? (_instance = new PoolManager());

	private Transform recycle;
    private TemplatePool<StringBuilder> strbudPool = new TemplatePool<StringBuilder>();
    private TemplatePool<AudioSource> audioSourcePool = new TemplatePool<AudioSource>();

    public void Start(Transform t)
    {
        recycle = t;
    }

	public void Clear()
    {
        strbudPool.Clear();
        audioSourcePool.Clear();
        if (recycle != null)
        {
            for (int i = 0; i < recycle.childCount; i++)
            {
                Object.Destroy(recycle.GetChild(i).gameObject);
            }
        }
    }

    public void PutInRecycle(Transform t)
    {
        t.SetParent(recycle);
        t.localPosition = Vector3.zero;
    }

    public StringBuilder GetBuilder()
    {
        StringBuilder builder = strbudPool.Get();
        if (builder == null)
        {
            builder = new StringBuilder();
        }
        else
        {
            builder.Clear();
        }
        return builder;
    }
    public string PutBuilder(StringBuilder builder, bool ret = true)
    {
        if (ret)
        {
            string str = builder.ToString();
            strbudPool.Put(builder);
            return str;
        }
        else
        {
            strbudPool.Put(builder);
            return string.Empty;
        }
    }

    public AudioSource GetAudioSource(out bool ret)
    {
        ret = false;
        AudioSource source = audioSourcePool.Get();
        if (source == null)
        {
            var obj = new GameObject("AudioSource");
            source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1.0f;
            source.dopplerLevel = 0;
            source.minDistance = 5.0f;
            source.maxDistance = 50.0f;
            ret = true;
        }
        return source;
    }
    public void PutAudioSource(AudioSource source)
    {
        if (source != null)
        {
            source.Stop();
            audioSourcePool.Put(source);
            source.transform.SetParent(recycle);
        }
    }

   public GameObject GetArea()
   {
       GameObject area = areaPool.Get();
       if (area == null)
       {
           area = new GameObject("Area");
       }
       return area;
   }
   public bool PutArea(GameObject obj)
   {
       bool ret = areaPool.Put(obj);
       if (ret)
       {
           obj.transform.SetParent(recycle);
           obj.transform.position = Vector3.zero;
           obj.transform.rotation = Quaternion.identity;
       }
       else
       {
           Object.Destroy(obj);
       }
       return ret;
   }

   public BulletInfo GetBulletInfo()
   {
       BulletInfo info = bulletInfoPool.Get();
       if (info == null)
       {
           info = new BulletInfo();
       }
       return info;
   }
   public bool PutBulletInfo(BulletInfo info)
   {
       bool ret = bulletInfoPool.Put(info);
       if (ret)
       {
           info.Reset();
       }
       return ret;
   }

   public BulletEntity GetBulletEntity(string name)
   {
       int bulletType = 0;
       if (name.IndexOf("Fire") != -1 || 
           name.IndexOf("Boom") != -1 || 
           name.IndexOf("Energy") != -1)
       {
           bulletType = 2;
       }
       else if (name.IndexOf("Whip") != -1)
       {
           bulletType = 3;
       }
       else if (name.IndexOf("Curvature") != -1)
       {
           bulletType = 4;
       }
       else if (name.IndexOf("Line") != -1 || name.IndexOf("laser") != -1)
       {
           bulletType = 1;
       }
       if (!bulletEntityPool.ContainsKey(bulletType))
       {
           bulletEntityPool[bulletType] = new TemplatePool<BulletEntity>();
       }

       BulletEntity entity = bulletEntityPool[bulletType].Get();
       if (entity != null)
       {
           return entity;
       }
       else
       {
           switch (bulletType)
           {
               case 0:
                   entity = new singlebulletentity();
                   break;
               case 1:
                   entity = new linebulletentity();
                   break;
               case 2:
                   entity = new particlebulletentity();
                   break;
               case 3:
                   entity = new whipbulletentity();
                   break;
               case 4:
                   entity = new curvaturebulletentity();
                   break;
           }
           entity.SetBulletType(bulletType);
       }
       return entity;
   }
   public bool PutBulletEntity(BulletEntity entity, int type)
   {
       if (!bulletEntityPool.ContainsKey(type))
       {
           bulletEntityPool[type] = new TemplatePool<BulletEntity>();
       }
       bool ret = bulletEntityPool[type].Put(entity);
       if (ret)
       {
           entity.Reset();
       }
       return ret;
   }

   public MonsterInfo GetMonsterInfo()
   {
       MonsterInfo info = monsterInfoPool.Get();
       if (info == null)
       {
           info = new MonsterInfo();
       }
       return info;
   }
   public bool PutMonsterInfo(MonsterInfo info)
   {
       bool ret = monsterInfoPool.Put(info);
       if (ret)
       {
           info.Reset();
       }
       return ret;
   }

   public MonsterEntity GetMonsterEntity()
   {
       MonsterEntity entity = monsterEntityPool.Get();
       if (entity != null)
       {
           entity.Live();
           return entity;
       }
       else
       {
           entity = new MonsterEntity();
       }
       return entity;
   }
   public bool PutMonsterEntity(MonsterEntity entity)
   {
       bool ret = monsterEntityPool.Put(entity);
       if (ret)
       {
           entity.Reset();
       }
       return ret;
   }

   public MagicEntity GetMagicEntity(ushort id, string name, BattleDefines.CoreType cType, uint frame)
   {
       MagicEntity entity = magicEntityPool.Get();
       if (entity != null)
       {
           entity.SetMagicEntity(id, name, cType, frame);
           return entity;
       }
       else
       {
           entity = new MagicEntity(id, name, cType, frame);
       }
       return entity;
   }
   public bool PutMagicEntity(MagicEntity entity)
   {
       bool ret = magicEntityPool.Put(entity);
       if (ret)
       {
           entity.Reset();
       }
       return ret;
   }

   public GameObject GetObject(PoolType type, string name)
   {
		GameObject obj = null;
		switch (type) {
			case PoolType.BULLET:
               if (bulletObjectPool.ContainsKey(name))
               {
                   obj = bulletObjectPool[name].Get();
               }
				break;
			case PoolType.MONSTER:
               if (monsterObjectPool.ContainsKey(name))
               {
                   obj = monsterObjectPool[name].Get();
               }
               break;
           case PoolType.MAGIC:
               if (magicObjectPool.ContainsKey(name))
               {
                   obj = magicObjectPool[name].Get();
               }
               break;
           case PoolType.ABILITY:
               if (abilityObjectPool.ContainsKey(name))
               {
                   obj = abilityObjectPool[name].Get();
               }
               break;
           case PoolType.UIEFFECT:
               if (uiEffectObjectPool.ContainsKey(name))
               {
                   obj = uiEffectObjectPool[name].Get();
               }
               break;
		}
		return obj;
	}
   public void PutObject(PoolType type, GameObject obj, string name)
   {
       bool ret = false;
       switch (type)
       {
           case PoolType.BULLET:
               if (!bulletObjectPool.ContainsKey(name))
               {
                   bulletObjectPool[name] = new TemplatePool<GameObject>();
               }
               ret = bulletObjectPool[name].Put(obj);
               if (ret)
               {
                   obj.transform.SetParent(recycle);
                   //obj.transform.position = Vector3.zero;
                   //obj.transform.rotation = Quaternion.identity;
                   //obj.transform.localScale = Vector3.one;
               }
               else
               {
                   Object.Destroy(obj);
               }
               break;
           case PoolType.MONSTER:
               if (!monsterObjectPool.ContainsKey(name))
               {
                   monsterObjectPool[name] = new TemplatePool<GameObject>();
               }
               ret = monsterObjectPool[name].Put(obj);
               if (ret)
               {
                   obj.transform.SetParent(recycle);
               }
               else
               {
                   Object.Destroy(obj);
               }
               break;
           case PoolType.MAGIC:
               if (!magicObjectPool.ContainsKey(name))
               {
                   magicObjectPool[name] = new TemplatePool<GameObject>();
               }
               ret = magicObjectPool[name].Put(obj);
               if (ret)
               {
                   obj.transform.SetParent(recycle);
               }
               else
               {
                   Object.Destroy(obj);
               }
               break;
           case PoolType.ABILITY:
               if (!abilityObjectPool.ContainsKey(name))
               {
                   abilityObjectPool[name] = new TemplatePool<GameObject>();
               }
               ret = abilityObjectPool[name].Put(obj);
               if (ret)
               {
                   obj.transform.SetParent(recycle);
               }
               else
               {
                   Object.Destroy(obj);
               }
               break;
           case PoolType.UIEFFECT:
               if (!uiEffectObjectPool.ContainsKey(name))
               {
                   uiEffectObjectPool[name] = new TemplatePool<GameObject>();
               }
               ret = uiEffectObjectPool[name].Put(obj);
               if (ret)
               {
                   obj.transform.SetParent(recycle);
               }
               else
               {
                   Object.Destroy(obj);
               }
               break;
       }
   }

   public GameObject GetWidgetObject(string name)
   {
       GameObject obj = null;
       if (monsterWidgetPool.ContainsKey(name))
       {
           obj = monsterWidgetPool[name].Get();
       }
       return obj;
   }
   public bool PutWidgetObject(GameObject obj, string name, bool isCanvas = false)
   {
       if (!monsterWidgetPool.ContainsKey(name))
       {
           monsterWidgetPool[name] = new TemplatePool<GameObject>();
       }
       bool ret = monsterWidgetPool[name].Put(obj);
       if (ret)
       {
           if (!isCanvas)
           {
               obj.transform.SetParent(recycle);
           }
           else
           {
               if (obj.activeSelf) obj.SetActive(false);
           }
       }
       else
       {
           Object.Destroy(obj);
       }
       return ret;
   }

   public AbilityInfo GetAbilityInfo()
   {
       AbilityInfo info = abilityInfoPool.Get();
       if (info == null)
       {
           info = new AbilityInfo();
       }
       return info;
   }
   public bool PutAbilityInfo(AbilityInfo info)
   {
       bool ret = abilityInfoPool.Put(info);
       if (ret)
       {
           info.Reset();
       }
       return ret;
   }

   public AbilityEntity GetAbilityEntity(int abilityType, uint id, string name, BattleDefines.CoreType type, uint frame)
   {
       if (!abilityEntityPool.ContainsKey(abilityType))
       {
           abilityEntityPool[abilityType] = new TemplatePool<AbilityEntity>();
       }

       AbilityEntity entity = abilityEntityPool[abilityType].Get();
       if (entity != null)
       {
           entity.SetAbilityEntity(id, name, type, frame);
           return entity;
       }
       else
       {
           switch (abilityType)
           {
               case 0:
                   entity = new AbilityEntity(id, name, type, frame);
                   break;
               case 1:
                   entity = new LineAbilityEntity(id, name, type, frame);
                   break;
               case 2:
                   entity = new CircleAbilityEntity(id, name, type, frame);
                   break;
               case 3:
                   entity = new LightningAbilityEntity(id, name, type, frame);
                   break;
               case 4:
                   entity = new DarkAbilityEntity(id, name, type, frame);
                   break;
           }
       }
       return entity;
   }
   public bool PutAbilityEntity(AbilityEntity entity, int type)
   {
       if (!abilityEntityPool.ContainsKey(type))
       {
           abilityEntityPool[type] = new TemplatePool<AbilityEntity>();
       }
       bool ret = abilityEntityPool[type].Put(entity);
       if (ret)
       {
           entity.Reset();
       }
       return ret;
   }
}

public class TemplatePool<T>
{
    private readonly Stack<T> stack = new Stack<T>();
    private int count = 0;
    private int maximum = -1;

    public TemplatePool(int max = -1)
    {
        maximum = max;
    }

    public T Get()
    {
        if (count == 0)
            return default;
        count--;
        var obj = stack.Pop();
        return obj;
    }

    public bool Put(T obj)
    {
        if (maximum != -1 && count >= maximum)
            return false;
        stack.Push(obj);
        count++;
        return true;
    }

    public void Clear()
    {
        stack.Clear();
        count = 0;
    }
}