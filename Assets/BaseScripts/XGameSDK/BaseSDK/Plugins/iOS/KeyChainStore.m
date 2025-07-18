//
//  KeyChainStore.m
//  Unity-iPhone
//
//  Created by System Administrator on 2022/10/9.
//

//参考：https://blog.csdn.net/u013121055/article/details/108354285

#import "KeyChainStore.h"
 
@implementation KeyChainStore
 
+ (NSMutableDictionary *)getKeychainQuery:(NSString *) service {
    return [NSMutableDictionary dictionaryWithObjectsAndKeys:
            (id)kSecClassGenericPassword,(id)kSecClass,
            service, (id)kSecAttrService,
            service, (id)kSecAttrAccount,
            (id)kSecAttrAccessibleAfterFirstUnlock,(id)kSecAttrAccessible,
            nil];
}
 
+ (void)save:(NSString *)service data:(id) data {
    //Get search dictionary
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    
    //Delete old item before add new item
    SecItemDelete((CFDictionaryRef)keychainQuery);
    
    //Add new object to search dictionary(Attention:the data format)
    [keychainQuery setObject:[NSKeyedArchiver archivedDataWithRootObject:data requiringSecureCoding:true error:NULL] forKey:(id)kSecValueData];

    //Add item to keychain with the search dictionary
    SecItemAdd((CFDictionaryRef)keychainQuery, NULL);
}
 
+ (id)load:(NSString *)service {
    id ret = nil;
    
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    //Configure the search setting
    //Since in our simple case we are expecting only a single attribute to be returned (the password) we can set the attribute kSecReturnData to kCFBooleanTrue
    [keychainQuery setObject:(id)kCFBooleanTrue forKey:(id)kSecReturnData];
    [keychainQuery setObject:(id)kSecMatchLimitOne forKey:(id)kSecMatchLimit];
    CFDataRef keyData = NULL;
    if (SecItemCopyMatching((CFDictionaryRef)keychainQuery, (CFTypeRef *)&keyData) == noErr) {
        @try {
            
            NSError *err = nil;
            //ret = [NSKeyedUnarchiver unarchivedObjectOfClasses:[NSSet setWithArray:@[NSArray.class,NSDictionary.class, NSString.class, UIFont.class, NSMutableArray.class, NSMutableDictionary.class, NSMutableString.class, UIColor.class, NSMutableData.class, NSData.class, NSNull.class, NSValue.class,NSDate.class]] fromData:data error:&err];
            
            ret = [NSKeyedUnarchiver unarchivedObjectOfClasses:[NSSet setWithArray:@[NSData.class, NSNull.class, NSString.class, NSMutableString.class]] fromData:(__bridge NSData *)keyData error:&err];
                
            //ret = [NSKeyedUnarchiver unarchiveObjectWithData:(__bridge NSData *)keyData];
        } @catch (NSException *e) {
            NSLog(@"Unarchive of %@ failed: %@", service, e);
        } @finally {
        }
    }
    if (keyData)
        CFRelease(keyData);
    return ret;
}
 
+ (void)deleteKeyData:(NSString *)service {
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    SecItemDelete((CFDictionaryRef)keychainQuery);
}
 
 
@end

