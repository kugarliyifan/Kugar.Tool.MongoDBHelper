�����ΪMongodb�ٷ��������ٶȷ�װ,�ṩ����ķ���ʹ�õ���

�ɵ�git��ʷ ,���Բ鿴 https://gitee.com/kugar/Kugar.Core/tree/master/Tool/Kugar.Tool.MongoDBHelper.NetCore

[![NuGet](https://img.shields.io/nuget/v/Kugar.Tool.MongoDBHelper.NetCore)](https://nuget.org/packages/Kugar.Tool.MongoDBHelper.NetCore)

1. ��start.cs �����

	```
	public Startup(IConfiguration configuration)
	{
		MongoDBContextHelper.RegisterSerializers(); //����ע���mongodb��ص����л���
	}
	

	```

2. �����Զ����Context��  
	```
	public class DeviceMongoDBContext
	{
			private IMongoDBContext _context = null;

			public DeviceMongoDBContext(
				string connUrl   // mongodb://localhost:27017/ �����ַ���
				,string dbName)  //���ݿ�����
			{
				_context=new MongoDBContextHelper(connUrl, dbName);
			}	


			public IMongoDBCollectionHelper<base_Device> base_Device => _context.GetCollection<base_Device>("base_Device");  //��ȡָ�����Ƶ�Collection

	}

	```