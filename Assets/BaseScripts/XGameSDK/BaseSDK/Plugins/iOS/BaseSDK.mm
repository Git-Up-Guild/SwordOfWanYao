
#import <AudioToolbox/AudioToolbox.h>
#import <Foundation/Foundation.h>
#import "UnityInterface.h"
#import "KeyChainStore.h"
#import "UnityAppController.h"

//在 KeyChain 中保存的设备ID字段名称
#define XGAME_IOS_DEVICE_UUID @"XGame_IOS_DEVICE_UUID"

//在 KeyChain 中保存的应用数据
#define XGAME_IOS_APP_DATA @"XGame_IOS_APP_DATA"

extern "C" {

    //保存应用数据
    void iosBaseSDK_SaveAplicationData(char* data)
    {
        if(data == NULL)
            return;

        NSString *strData = [NSString stringWithUTF8String:data];

        //将该uuid保存到keychain
        [KeyChainStore save:XGAME_IOS_APP_DATA data:strData];
         
        NSLog(@"保存XGAME_IOS_APP_DATA:%@", strData);
    }

    //获取应用数据
    char* iosBaseSDK_LoadAplicationData()
    {
        NSString * strData = (NSString *)[KeyChainStore load:XGAME_IOS_APP_DATA];
     
        if ([strData isEqualToString:@""] || !strData)
        {
            NSLog(@"加载XGAME_IOS_APP_DATA失败，数据不存在！");
            char* ret = (char*)malloc(1);
            ret[0] = '\0';
            return ret;
        }
        else
        {
            const char * data = [strData UTF8String];
         
            NSLog(@"加载XGAME_IOS_APP_DATA:%@", strData);
         
            //拷贝一份，因为Unity会释主动释放这个字符串
            char* ret = (char*)malloc(strlen(data) + 1);
            strcpy(ret, data);
         
            return ret;
        }

    }
	
		
	//加载交易数据
	char* iosBaseSDK_LoadAplicationDataWithKey(char* key)
	{
		if(key == NULL)
		{
			char* ret = (char*)malloc(1);
            ret[0] = '\0';
			return ret;
		}
			
		NSString* strKey = [NSString stringWithUTF8String:key];
		NSString* strData = (NSString*)[KeyChainStore load:strKey];
     
        if ([strData isEqualToString:@""] || !strData)
        {
            NSLog(@"加载%@失败：数据不存在！", strKey);
            char* ret = (char*)malloc(1);
            ret[0] = '\0';
            return ret;
        }
        else
        {
            const char * data = [strData UTF8String];
         
			NSLog(@"加载%@完成：%@", strKey, strData);
         
            //拷贝一份，因为Unity会释主动释放这个字符串
            char* ret = (char*)malloc(strlen(data) + 1);
            strcpy(ret, data);
         
            return ret;
        }
	}
	
	//保存交易数据
	void iosBaseSDK_SaveAplicationDataWithKey(char* key, char* data)
	{
		if(data == NULL || key == NULL)
            return;

        NSString *strKey = [NSString stringWithUTF8String:key];
		NSString *strData = [NSString stringWithUTF8String:data];

        //将该uuid保存到keychain
        [KeyChainStore save:strKey data:strData];
         
        NSLog(@"保存%@完成:%@", strKey, strData);
	}

    char* iosBaseSDK_GetSafeAreaInsets()
	{
        UIViewController* viewController = UnityGetGLViewController();
        
        //构建json字符串
        NSString* jsonString = [NSString stringWithFormat:@"{\"top\":%f,\"bottom\":%f,\"left\":%f,\"right\":%f}", viewController.view.safeAreaInsets.top, viewController.view.safeAreaInsets.bottom, viewController.view.safeAreaInsets.left, viewController.view.safeAreaInsets.right];
        
        NSLog(@"iosBaseSDK_GetSafeAreaInsets jsonString: %@", jsonString);
 
        // nsstring -> const char*
        const char* retStr = [jsonString UTF8String];
        
        //拷贝一份，因为Unity会释主动释放这个字符串
        char* ret = (char*)malloc(strlen(retStr) + 1);
        strcpy(ret, retStr);
        
        return ret;
	}

	char* iosBaseSDK_GenerateDeviceID()
	{
        NSString * strUUID = (NSString *)[KeyChainStore load:XGAME_IOS_DEVICE_UUID];
        
        //首次执行该方法时，uuid为空
        if ([strUUID isEqualToString:@""] || !strUUID)
        {
            //生成一个uuid的方法
            CFUUIDRef uuidRef = CFUUIDCreate(kCFAllocatorDefault);
            
            strUUID = (NSString *)CFBridgingRelease(CFUUIDCreateString (kCFAllocatorDefault, uuidRef));
            
            //将该uuid保存到keychain
            [KeyChainStore save:XGAME_IOS_DEVICE_UUID data:strUUID];
            
            NSLog(@"生成XGAME_IOS_DEVICE_UUID:%@",strUUID);
        }
        else
        {
            NSLog(@"已经存在XGame_IOS_DEVICE_UUID:%@",strUUID);
        }
        
        // nsstring -> const char*
        const char* retStr = [strUUID UTF8String];
        
        //拷贝一份，因为Unity会释主动释放这个字符串
        char* ret = (char*)malloc(strlen(retStr) + 1);
        strcpy(ret, retStr);
        
        return ret;
	}

    void iosBaseSDK_VibratorApp(long delay, long time) 
    {

    /*
        AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);//默认震动效果
        如果想要其他震动效果，可参考：
        // 普通短震，3D Touch 中 Pop 震动反馈
        AudioServicesPlaySystemSound(1520);
        // 普通短震，3D Touch 中 Peek 震动反馈
        AudioServicesPlaySystemSound(1519);
        // 连续三次短震
        AudioServicesPlaySystemSound(1521);
    */
        AudioServicesPlaySystemSound(1519);
    }

    void iosBaseSDK_SetIdleTimerDisabled() 
    {
        [[UIApplication sharedApplication] setIdleTimerDisabled: NO];
        [[UIApplication sharedApplication] setIdleTimerDisabled: YES];
    }
	
	//退出程序
	void iosBaseSDK_ExitApplication() 
    {
        UnityAppController *app = [UIApplication sharedApplication].delegate;
        UIWindow *window = app.window;
        
        [UIView animateWithDuration:1.0f animations:^{
                    window.alpha = 0;
            } completion:^(BOOL finished) {
                    exit(0);
            }];
    }
	
	
    void uncaughtExceptionHandler(NSException * exception)
    {
        //获取系统当前时间，（注：用[NSDate date]直接获取的是格林尼治时间，有时差）
        NSDateFormatter *formatter =[[NSDateFormatter alloc] init];
        [formatter setDateFormat:@"yyyy-MM-dd HH:mm:ss"];
        NSString *crashTime = [formatter stringFromDate:[NSDate date]];
        [formatter setDateFormat:@"HH-mm-ss"];
        NSString *crashTimeStr = [formatter stringFromDate:[NSDate date]];
        [formatter setDateFormat:@"yyyyMMdd"];
        
        NSString *crashDate = [formatter stringFromDate:[NSDate date]];
        //异常的堆栈信息
        NSArray *stackArray = [exception callStackSymbols];
        //出现异常的原因
        NSString *reason = [exception reason];
        //异常名称
        NSString *name = [exception name];
        //拼接错误信息
        NSString *exceptionInfo = [NSString stringWithFormat:@"CrashTime: %@\nException reason: %@\nException name: %@\nException call stack:%@\n", crashTime, name, reason, stackArray];

        //把错误信息保存到本地文件，设置errorLogPath路径下
        //并且经试验，此方法写入本地文件有效。
            
            
        
        
        NSString *errorLogPath = [NSString stringWithFormat:@"%@/CrashLogs/%@/", NSHomeDirectory(), crashDate];
        NSFileManager *manager = [NSFileManager defaultManager];
        if (![manager fileExistsAtPath:errorLogPath]) {
            [manager createDirectoryAtPath:errorLogPath withIntermediateDirectories:true attributes:nil error:nil];
        }
        
        errorLogPath = [errorLogPath stringByAppendingFormat:@"%@.log",crashTimeStr];
        NSError *error = nil;
        NSLog(@"%@", errorLogPath);
        BOOL isSuccess = [exceptionInfo writeToFile:errorLogPath atomically:YES encoding:NSUTF8StringEncoding error:&error];
        if (!isSuccess) {
            NSLog(@"将crash信息保存到本地失败: %@", error.userInfo);
        }
    }

    void iosBaseSDK_SetupExceptionHandler()
    {
         NSSetUncaughtExceptionHandler(&uncaughtExceptionHandler);
    }

}
