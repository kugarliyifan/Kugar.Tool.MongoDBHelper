该类库为Mongodb官方驱动的再度封装,提供更多的方便使用的类

旧的git历史 ,可以查看 https://gitee.com/kugar/Kugar.Core/tree/master/Tool/Kugar.Tool.MongoDBHelper.NetCore

[![NuGet](https://img.shields.io/nuget/v/Kugar.Tool.MongoDBHelper.NetCore)](https://nuget.org/packages/Kugar.Tool.MongoDBHelper.NetCore)

1. 在start.cs 中添加

	```
	public Startup(IConfiguration configuration)
	{
		MongoDBContextHelper.RegisterSerializers(); //用于注册跟mongodb相关的序列化器
	}
	

	```

2. 构造自定义的Context类  
	```
	public class DeviceMongoDBContext
	{
			private IMongoDBContext _context = null;

			public DeviceMongoDBContext(
				string connUrl   // mongodb://localhost:27017/ 链接字符串
				,string dbName)  //数据库链接
			{
				_context=new MongoDBContextHelper(connUrl, dbName);
			}	


			public IMongoDBCollectionHelper<base_Device> base_Device => _context.GetCollection<base_Device>("base_Device");  //获取指定名称的Collection

	}

	```