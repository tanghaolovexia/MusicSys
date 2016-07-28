/*
Navicat SQLite Data Transfer

Source Server         : MusicSqlLite
Source Server Version : 30714
Source Host           : :0

Target Server Type    : SQLite
Target Server Version : 30714
File Encoding         : 65001

Date: 2016-07-18 20:25:37
*/

PRAGMA foreign_keys = OFF;

-- ----------------------------
-- Table structure for tb_SaveMusicList
-- ----------------------------
DROP TABLE IF EXISTS "main"."tb_SaveMusicList";
CREATE TABLE "tb_SaveMusicList" (
"ID"  INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
"Name"  TEXT(100),
"SingLength"  TEXT(5),
"Singer"  TEXT(20)
);

-- ----------------------------
-- Records of tb_SaveMusicList
-- ----------------------------
INSERT INTO "main"."tb_SaveMusicList" VALUES (1, '我的心受伤了', 3.45, '张学友');
INSERT INTO "main"."tb_SaveMusicList" VALUES (2, '我的歌', 5.56, '唐浩');
