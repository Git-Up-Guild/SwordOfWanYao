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
 
//��ȡ�ֶ�
+ (void)save:(NSString *)service data:(id)data;

//�����ֶ�
+ (id)load:(NSString *)service;

//ɾ���ֶ�
+ (void)deleteKeyData:(NSString *)service;
 
@end

#endif /* KeyChainStore_h */
