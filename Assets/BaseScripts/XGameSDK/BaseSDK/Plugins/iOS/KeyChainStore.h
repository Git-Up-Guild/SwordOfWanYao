//
//  KeyChainStore.h
//  Unity-iPhone
//
//  Created by System Administrator on 2022/10/9.
//

#ifndef KeyChainStore_h
#define KeyChainStore_h

#import <Foundation/Foundation.h>
 
@interface KeyChainStore : NSObject
 
//¶ÁÈ¡×Ö¶Î
+ (void)save:(NSString *)service data:(id)data;

//¼ÓÔØ×Ö¶Î
+ (id)load:(NSString *)service;

//É¾³ý×Ö¶Î
+ (void)deleteKeyData:(NSString *)service;
 
@end

#endif /* KeyChainStore_h */
