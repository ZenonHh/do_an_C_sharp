; ModuleID = 'marshal_methods.x86.ll'
source_filename = "marshal_methods.x86.ll"
target datalayout = "e-m:e-p:32:32-p270:32:32-p271:32:32-p272:64:64-f64:32:64-f80:32-n8:16:32-S128"
target triple = "i686-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [340 x ptr] zeroinitializer, align 4

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [674 x i32] [
	i32 2616222, ; 0: System.Net.NetworkInformation.dll => 0x27eb9e => 68
	i32 10166715, ; 1: System.Net.NameResolution.dll => 0x9b21bb => 67
	i32 15721112, ; 2: System.Runtime.Intrinsics.dll => 0xefe298 => 108
	i32 32687329, ; 3: Xamarin.AndroidX.Lifecycle.Runtime => 0x1f2c4e1 => 256
	i32 34715100, ; 4: Xamarin.Google.Guava.ListenableFuture.dll => 0x211b5dc => 290
	i32 34839235, ; 5: System.IO.FileSystem.DriveInfo => 0x2139ac3 => 48
	i32 39485524, ; 6: System.Net.WebSockets.dll => 0x25a8054 => 80
	i32 42639949, ; 7: System.Threading.Thread => 0x28aa24d => 145
	i32 66541672, ; 8: System.Diagnostics.StackTrace => 0x3f75868 => 30
	i32 67008169, ; 9: zh-Hant\Microsoft.Maui.Controls.resources => 0x3fe76a9 => 335
	i32 68219467, ; 10: System.Security.Cryptography.Primitives => 0x410f24b => 124
	i32 72070932, ; 11: Microsoft.Maui.Graphics.dll => 0x44bb714 => 199
	i32 82292897, ; 12: System.Runtime.CompilerServices.VisualC.dll => 0x4e7b0a1 => 102
	i32 101534019, ; 13: Xamarin.AndroidX.SlidingPaneLayout => 0x60d4943 => 274
	i32 117431740, ; 14: System.Runtime.InteropServices => 0x6ffddbc => 107
	i32 120558881, ; 15: Xamarin.AndroidX.SlidingPaneLayout.dll => 0x72f9521 => 274
	i32 122350210, ; 16: System.Threading.Channels.dll => 0x74aea82 => 139
	i32 134690465, ; 17: Xamarin.Kotlin.StdLib.Jdk7.dll => 0x80736a1 => 298
	i32 142721839, ; 18: System.Net.WebHeaderCollection => 0x881c32f => 77
	i32 149764678, ; 19: Svg.Skia.dll => 0x8ed3a46 => 213
	i32 149972175, ; 20: System.Security.Cryptography.Primitives.dll => 0x8f064cf => 124
	i32 159306688, ; 21: System.ComponentModel.Annotations => 0x97ed3c0 => 13
	i32 163002283, ; 22: DoAnCSharp.dll => 0x9b737ab => 0
	i32 165246403, ; 23: Xamarin.AndroidX.Collection.dll => 0x9d975c3 => 230
	i32 176265551, ; 24: System.ServiceProcess => 0xa81994f => 132
	i32 182336117, ; 25: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0xade3a75 => 276
	i32 184328833, ; 26: System.ValueTuple.dll => 0xafca281 => 151
	i32 195452805, ; 27: vi/Microsoft.Maui.Controls.resources.dll => 0xba65f85 => 332
	i32 199333315, ; 28: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xbe195c3 => 333
	i32 205061960, ; 29: System.ComponentModel => 0xc38ff48 => 18
	i32 209399409, ; 30: Xamarin.AndroidX.Browser.dll => 0xc7b2e71 => 228
	i32 220171995, ; 31: System.Diagnostics.Debug => 0xd1f8edb => 26
	i32 230216969, ; 32: Xamarin.AndroidX.Legacy.Support.Core.Utils.dll => 0xdb8d509 => 250
	i32 230752869, ; 33: Microsoft.CSharp.dll => 0xdc10265 => 1
	i32 231409092, ; 34: System.Linq.Parallel => 0xdcb05c4 => 59
	i32 231814094, ; 35: System.Globalization => 0xdd133ce => 42
	i32 246610117, ; 36: System.Reflection.Emit.Lightweight => 0xeb2f8c5 => 91
	i32 261689757, ; 37: Xamarin.AndroidX.ConstraintLayout.dll => 0xf99119d => 233
	i32 276479776, ; 38: System.Threading.Timer.dll => 0x107abf20 => 147
	i32 278686392, ; 39: Xamarin.AndroidX.Lifecycle.LiveData.dll => 0x109c6ab8 => 252
	i32 280482487, ; 40: Xamarin.AndroidX.Interpolator => 0x10b7d2b7 => 249
	i32 280992041, ; 41: cs/Microsoft.Maui.Controls.resources.dll => 0x10bf9929 => 304
	i32 291076382, ; 42: System.IO.Pipes.AccessControl.dll => 0x1159791e => 54
	i32 292822316, ; 43: Mapsui.UI.Maui => 0x11741d2c => 181
	i32 298918909, ; 44: System.Net.Ping.dll => 0x11d123fd => 69
	i32 317674968, ; 45: vi\Microsoft.Maui.Controls.resources => 0x12ef55d8 => 332
	i32 318968648, ; 46: Xamarin.AndroidX.Activity.dll => 0x13031348 => 219
	i32 321597661, ; 47: System.Numerics => 0x132b30dd => 83
	i32 336156722, ; 48: ja/Microsoft.Maui.Controls.resources.dll => 0x14095832 => 317
	i32 342366114, ; 49: Xamarin.AndroidX.Lifecycle.Common => 0x146817a2 => 251
	i32 356389973, ; 50: it/Microsoft.Maui.Controls.resources.dll => 0x153e1455 => 316
	i32 360082299, ; 51: System.ServiceModel.Web => 0x15766b7b => 131
	i32 367780167, ; 52: System.IO.Pipes => 0x15ebe147 => 55
	i32 374914964, ; 53: System.Transactions.Local => 0x1658bf94 => 149
	i32 375677976, ; 54: System.Net.ServicePoint.dll => 0x16646418 => 74
	i32 379916513, ; 55: System.Threading.Thread.dll => 0x16a510e1 => 145
	i32 385762202, ; 56: System.Memory.dll => 0x16fe439a => 62
	i32 392610295, ; 57: System.Threading.ThreadPool.dll => 0x1766c1f7 => 146
	i32 395744057, ; 58: _Microsoft.Android.Resource.Designer => 0x17969339 => 336
	i32 403441872, ; 59: WindowsBase => 0x180c08d0 => 165
	i32 435591531, ; 60: sv/Microsoft.Maui.Controls.resources.dll => 0x19f6996b => 328
	i32 441335492, ; 61: Xamarin.AndroidX.ConstraintLayout.Core => 0x1a4e3ec4 => 234
	i32 442565967, ; 62: System.Collections => 0x1a61054f => 12
	i32 450948140, ; 63: Xamarin.AndroidX.Fragment.dll => 0x1ae0ec2c => 247
	i32 451504562, ; 64: System.Security.Cryptography.X509Certificates => 0x1ae969b2 => 125
	i32 456227837, ; 65: System.Web.HttpUtility.dll => 0x1b317bfd => 152
	i32 459347974, ; 66: System.Runtime.Serialization.Primitives.dll => 0x1b611806 => 113
	i32 465658307, ; 67: ExCSS => 0x1bc161c3 => 177
	i32 465846621, ; 68: mscorlib => 0x1bc4415d => 166
	i32 469710990, ; 69: System.dll => 0x1bff388e => 164
	i32 469965489, ; 70: Svg.Model => 0x1c031ab1 => 212
	i32 476646585, ; 71: Xamarin.AndroidX.Interpolator.dll => 0x1c690cb9 => 249
	i32 486930444, ; 72: Xamarin.AndroidX.LocalBroadcastManager.dll => 0x1d05f80c => 262
	i32 498788369, ; 73: System.ObjectModel => 0x1dbae811 => 84
	i32 500358224, ; 74: id/Microsoft.Maui.Controls.resources.dll => 0x1dd2dc50 => 315
	i32 503918385, ; 75: fi/Microsoft.Maui.Controls.resources.dll => 0x1e092f31 => 309
	i32 513247710, ; 76: Microsoft.Extensions.Primitives.dll => 0x1e9789de => 192
	i32 525008092, ; 77: SkiaSharp.dll => 0x1f4afcdc => 205
	i32 526420162, ; 78: System.Transactions.dll => 0x1f6088c2 => 150
	i32 527452488, ; 79: Xamarin.Kotlin.StdLib.Jdk7 => 0x1f704948 => 298
	i32 530272170, ; 80: System.Linq.Queryable => 0x1f9b4faa => 60
	i32 539058512, ; 81: Microsoft.Extensions.Logging => 0x20216150 => 189
	i32 540030774, ; 82: System.IO.FileSystem.dll => 0x20303736 => 51
	i32 545304856, ; 83: System.Runtime.Extensions => 0x2080b118 => 103
	i32 546455878, ; 84: System.Runtime.Serialization.Xml => 0x20924146 => 114
	i32 549171840, ; 85: System.Globalization.Calendars => 0x20bbb280 => 40
	i32 557405415, ; 86: Jsr305Binding => 0x213954e7 => 287
	i32 569601784, ; 87: Xamarin.AndroidX.Window.Extensions.Core.Core => 0x21f36ef8 => 285
	i32 577335427, ; 88: System.Security.Cryptography.Cng => 0x22697083 => 120
	i32 592146354, ; 89: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x234b6fb2 => 323
	i32 597488923, ; 90: CommunityToolkit.Maui => 0x239cf51b => 174
	i32 601371474, ; 91: System.IO.IsolatedStorage.dll => 0x23d83352 => 52
	i32 605376203, ; 92: System.IO.Compression.FileSystem => 0x24154ecb => 44
	i32 613668793, ; 93: System.Security.Cryptography.Algorithms => 0x2493d7b9 => 119
	i32 627609679, ; 94: Xamarin.AndroidX.CustomView => 0x2568904f => 239
	i32 627931235, ; 95: nl\Microsoft.Maui.Controls.resources => 0x256d7863 => 321
	i32 639843206, ; 96: Xamarin.AndroidX.Emoji2.ViewsHelper.dll => 0x26233b86 => 245
	i32 643868501, ; 97: System.Net => 0x2660a755 => 81
	i32 662205335, ; 98: System.Text.Encodings.Web.dll => 0x27787397 => 136
	i32 663517072, ; 99: Xamarin.AndroidX.VersionedParcelable => 0x278c7790 => 281
	i32 666292255, ; 100: Xamarin.AndroidX.Arch.Core.Common.dll => 0x27b6d01f => 226
	i32 672442732, ; 101: System.Collections.Concurrent => 0x2814a96c => 8
	i32 680049820, ; 102: Mapsui.Rendering.Skia.dll => 0x2888bc9c => 183
	i32 683518922, ; 103: System.Net.Security => 0x28bdabca => 73
	i32 688181140, ; 104: ca/Microsoft.Maui.Controls.resources.dll => 0x2904cf94 => 303
	i32 690569205, ; 105: System.Xml.Linq.dll => 0x29293ff5 => 155
	i32 691348768, ; 106: Xamarin.KotlinX.Coroutines.Android.dll => 0x29352520 => 300
	i32 693804605, ; 107: System.Windows => 0x295a9e3d => 154
	i32 699345723, ; 108: System.Reflection.Emit => 0x29af2b3b => 92
	i32 700284507, ; 109: Xamarin.Jetbrains.Annotations => 0x29bd7e5b => 295
	i32 700358131, ; 110: System.IO.Compression.ZipFile => 0x29be9df3 => 45
	i32 706645707, ; 111: ko/Microsoft.Maui.Controls.resources.dll => 0x2a1e8ecb => 318
	i32 709557578, ; 112: de/Microsoft.Maui.Controls.resources.dll => 0x2a4afd4a => 306
	i32 720511267, ; 113: Xamarin.Kotlin.StdLib.Jdk8 => 0x2af22123 => 299
	i32 722857257, ; 114: System.Runtime.Loader.dll => 0x2b15ed29 => 109
	i32 735137430, ; 115: System.Security.SecureString.dll => 0x2bd14e96 => 129
	i32 752232764, ; 116: System.Diagnostics.Contracts.dll => 0x2cd6293c => 25
	i32 755313932, ; 117: Xamarin.Android.Glide.Annotations.dll => 0x2d052d0c => 216
	i32 759454413, ; 118: System.Net.Requests => 0x2d445acd => 72
	i32 762598435, ; 119: System.IO.Pipes.dll => 0x2d745423 => 55
	i32 775507847, ; 120: System.IO.Compression => 0x2e394f87 => 46
	i32 777317022, ; 121: sk\Microsoft.Maui.Controls.resources => 0x2e54ea9e => 327
	i32 778756650, ; 122: SkiaSharp.HarfBuzz.dll => 0x2e6ae22a => 206
	i32 789151979, ; 123: Microsoft.Extensions.Options => 0x2f0980eb => 191
	i32 790371945, ; 124: Xamarin.AndroidX.CustomView.PoolingContainer.dll => 0x2f1c1e69 => 240
	i32 804715423, ; 125: System.Data.Common => 0x2ff6fb9f => 22
	i32 807930345, ; 126: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx.dll => 0x302809e9 => 254
	i32 823281589, ; 127: System.Private.Uri.dll => 0x311247b5 => 86
	i32 830298997, ; 128: System.IO.Compression.Brotli => 0x317d5b75 => 43
	i32 832635846, ; 129: System.Xml.XPath.dll => 0x31a103c6 => 160
	i32 834051424, ; 130: System.Net.Quic => 0x31b69d60 => 71
	i32 843511501, ; 131: Xamarin.AndroidX.Print => 0x3246f6cd => 267
	i32 873119928, ; 132: Microsoft.VisualBasic => 0x340ac0b8 => 3
	i32 877678880, ; 133: System.Globalization.dll => 0x34505120 => 42
	i32 878954865, ; 134: System.Net.Http.Json => 0x3463c971 => 63
	i32 899130691, ; 135: NetTopologySuite.dll => 0x3597a543 => 201
	i32 904024072, ; 136: System.ComponentModel.Primitives.dll => 0x35e25008 => 16
	i32 908888060, ; 137: Microsoft.Maui.Maps => 0x362c87fc => 200
	i32 911108515, ; 138: System.IO.MemoryMappedFiles.dll => 0x364e69a3 => 53
	i32 926902833, ; 139: tr/Microsoft.Maui.Controls.resources.dll => 0x373f6a31 => 330
	i32 928116545, ; 140: Xamarin.Google.Guava.ListenableFuture => 0x3751ef41 => 290
	i32 952186615, ; 141: System.Runtime.InteropServices.JavaScript.dll => 0x38c136f7 => 105
	i32 956575887, ; 142: Xamarin.Kotlin.StdLib.Jdk8.dll => 0x3904308f => 299
	i32 966729478, ; 143: Xamarin.Google.Crypto.Tink.Android => 0x399f1f06 => 288
	i32 967690846, ; 144: Xamarin.AndroidX.Lifecycle.Common.dll => 0x39adca5e => 251
	i32 975236339, ; 145: System.Diagnostics.Tracing => 0x3a20ecf3 => 34
	i32 975874589, ; 146: System.Xml.XDocument => 0x3a2aaa1d => 158
	i32 986514023, ; 147: System.Private.DataContractSerialization.dll => 0x3acd0267 => 85
	i32 987214855, ; 148: System.Diagnostics.Tools => 0x3ad7b407 => 32
	i32 992768348, ; 149: System.Collections.dll => 0x3b2c715c => 12
	i32 994442037, ; 150: System.IO.FileSystem => 0x3b45fb35 => 51
	i32 1001831731, ; 151: System.IO.UnmanagedMemoryStream.dll => 0x3bb6bd33 => 56
	i32 1012816738, ; 152: Xamarin.AndroidX.SavedState.dll => 0x3c5e5b62 => 271
	i32 1019214401, ; 153: System.Drawing => 0x3cbffa41 => 36
	i32 1028951442, ; 154: Microsoft.Extensions.DependencyInjection.Abstractions => 0x3d548d92 => 188
	i32 1029334545, ; 155: da/Microsoft.Maui.Controls.resources.dll => 0x3d5a6611 => 305
	i32 1031528504, ; 156: Xamarin.Google.ErrorProne.Annotations.dll => 0x3d7be038 => 289
	i32 1035644815, ; 157: Xamarin.AndroidX.AppCompat => 0x3dbaaf8f => 224
	i32 1036536393, ; 158: System.Drawing.Primitives.dll => 0x3dc84a49 => 35
	i32 1044663988, ; 159: System.Linq.Expressions.dll => 0x3e444eb4 => 58
	i32 1052210849, ; 160: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x3eb776a1 => 258
	i32 1067306892, ; 161: GoogleGson => 0x3f9dcf8c => 178
	i32 1082857460, ; 162: System.ComponentModel.TypeConverter => 0x408b17f4 => 17
	i32 1084122840, ; 163: Xamarin.Kotlin.StdLib => 0x409e66d8 => 296
	i32 1098259244, ; 164: System => 0x41761b2c => 164
	i32 1118262833, ; 165: ko\Microsoft.Maui.Controls.resources => 0x42a75631 => 318
	i32 1121599056, ; 166: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.dll => 0x42da3e50 => 257
	i32 1149092582, ; 167: Xamarin.AndroidX.Window => 0x447dc2e6 => 284
	i32 1168523401, ; 168: pt\Microsoft.Maui.Controls.resources => 0x45a64089 => 324
	i32 1170634674, ; 169: System.Web.dll => 0x45c677b2 => 153
	i32 1175144683, ; 170: Xamarin.AndroidX.VectorDrawable.Animated => 0x460b48eb => 280
	i32 1178241025, ; 171: Xamarin.AndroidX.Navigation.Runtime.dll => 0x463a8801 => 265
	i32 1203215381, ; 172: pl/Microsoft.Maui.Controls.resources.dll => 0x47b79c15 => 322
	i32 1204270330, ; 173: Xamarin.AndroidX.Arch.Core.Common => 0x47c7b4fa => 226
	i32 1208641965, ; 174: System.Diagnostics.Process => 0x480a69ad => 29
	i32 1214827643, ; 175: CommunityToolkit.Mvvm => 0x4868cc7b => 176
	i32 1219128291, ; 176: System.IO.IsolatedStorage => 0x48aa6be3 => 52
	i32 1234928153, ; 177: nb/Microsoft.Maui.Controls.resources.dll => 0x499b8219 => 320
	i32 1243150071, ; 178: Xamarin.AndroidX.Window.Extensions.Core.Core.dll => 0x4a18f6f7 => 285
	i32 1253011324, ; 179: Microsoft.Win32.Registry => 0x4aaf6f7c => 5
	i32 1260983243, ; 180: cs\Microsoft.Maui.Controls.resources => 0x4b2913cb => 304
	i32 1264511973, ; 181: Xamarin.AndroidX.Startup.StartupRuntime.dll => 0x4b5eebe5 => 275
	i32 1267360935, ; 182: Xamarin.AndroidX.VectorDrawable => 0x4b8a64a7 => 279
	i32 1273260888, ; 183: Xamarin.AndroidX.Collection.Ktx => 0x4be46b58 => 231
	i32 1275534314, ; 184: Xamarin.KotlinX.Coroutines.Android => 0x4c071bea => 300
	i32 1278448581, ; 185: Xamarin.AndroidX.Annotation.Jvm => 0x4c3393c5 => 223
	i32 1293217323, ; 186: Xamarin.AndroidX.DrawerLayout.dll => 0x4d14ee2b => 242
	i32 1309188875, ; 187: System.Private.DataContractSerialization => 0x4e08a30b => 85
	i32 1313028017, ; 188: Topten.RichTextKit => 0x4e4337b1 => 214
	i32 1322716291, ; 189: Xamarin.AndroidX.Window.dll => 0x4ed70c83 => 284
	i32 1324164729, ; 190: System.Linq => 0x4eed2679 => 61
	i32 1335329327, ; 191: System.Runtime.Serialization.Json.dll => 0x4f97822f => 112
	i32 1364015309, ; 192: System.IO => 0x514d38cd => 57
	i32 1373134921, ; 193: zh-Hans\Microsoft.Maui.Controls.resources => 0x51d86049 => 334
	i32 1376866003, ; 194: Xamarin.AndroidX.SavedState => 0x52114ed3 => 271
	i32 1379779777, ; 195: System.Resources.ResourceManager => 0x523dc4c1 => 99
	i32 1388087747, ; 196: Mapsui.dll => 0x52bc89c3 => 180
	i32 1402170036, ; 197: System.Configuration.dll => 0x53936ab4 => 19
	i32 1406073936, ; 198: Xamarin.AndroidX.CoordinatorLayout => 0x53cefc50 => 235
	i32 1408764838, ; 199: System.Runtime.Serialization.Formatters.dll => 0x53f80ba6 => 111
	i32 1411638395, ; 200: System.Runtime.CompilerServices.Unsafe => 0x5423e47b => 101
	i32 1422545099, ; 201: System.Runtime.CompilerServices.VisualC => 0x54ca50cb => 102
	i32 1422967952, ; 202: Mapsui.Tiling.dll => 0x54d0c490 => 184
	i32 1430672901, ; 203: ar\Microsoft.Maui.Controls.resources => 0x55465605 => 302
	i32 1434145427, ; 204: System.Runtime.Handles => 0x557b5293 => 104
	i32 1435222561, ; 205: Xamarin.Google.Crypto.Tink.Android.dll => 0x558bc221 => 288
	i32 1439761251, ; 206: System.Net.Quic.dll => 0x55d10363 => 71
	i32 1443938015, ; 207: NetTopologySuite => 0x5610bedf => 201
	i32 1452070440, ; 208: System.Formats.Asn1.dll => 0x568cd628 => 38
	i32 1453312822, ; 209: System.Diagnostics.Tools.dll => 0x569fcb36 => 32
	i32 1457743152, ; 210: System.Runtime.Extensions.dll => 0x56e36530 => 103
	i32 1458022317, ; 211: System.Net.Security.dll => 0x56e7a7ad => 73
	i32 1461004990, ; 212: es\Microsoft.Maui.Controls.resources => 0x57152abe => 308
	i32 1461234159, ; 213: System.Collections.Immutable.dll => 0x5718a9ef => 9
	i32 1461719063, ; 214: System.Security.Cryptography.OpenSsl => 0x57201017 => 123
	i32 1462112819, ; 215: System.IO.Compression.dll => 0x57261233 => 46
	i32 1469204771, ; 216: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x57924923 => 225
	i32 1470490898, ; 217: Microsoft.Extensions.Primitives => 0x57a5e912 => 192
	i32 1479771757, ; 218: System.Collections.Immutable => 0x5833866d => 9
	i32 1480492111, ; 219: System.IO.Compression.Brotli.dll => 0x583e844f => 43
	i32 1487239319, ; 220: Microsoft.Win32.Primitives => 0x58a57897 => 4
	i32 1490025113, ; 221: Xamarin.AndroidX.SavedState.SavedState.Ktx.dll => 0x58cffa99 => 272
	i32 1493001747, ; 222: hi/Microsoft.Maui.Controls.resources.dll => 0x58fd6613 => 312
	i32 1514721132, ; 223: el/Microsoft.Maui.Controls.resources.dll => 0x5a48cf6c => 307
	i32 1536373174, ; 224: System.Diagnostics.TextWriterTraceListener => 0x5b9331b6 => 31
	i32 1543031311, ; 225: System.Text.RegularExpressions.dll => 0x5bf8ca0f => 138
	i32 1543355203, ; 226: System.Reflection.Emit.dll => 0x5bfdbb43 => 92
	i32 1550322496, ; 227: System.Reflection.Extensions.dll => 0x5c680b40 => 93
	i32 1551623176, ; 228: sk/Microsoft.Maui.Controls.resources.dll => 0x5c7be408 => 327
	i32 1565862583, ; 229: System.IO.FileSystem.Primitives => 0x5d552ab7 => 49
	i32 1566207040, ; 230: System.Threading.Tasks.Dataflow.dll => 0x5d5a6c40 => 141
	i32 1573704789, ; 231: System.Runtime.Serialization.Json => 0x5dccd455 => 112
	i32 1580037396, ; 232: System.Threading.Overlapped => 0x5e2d7514 => 140
	i32 1582372066, ; 233: Xamarin.AndroidX.DocumentFile.dll => 0x5e5114e2 => 241
	i32 1592978981, ; 234: System.Runtime.Serialization.dll => 0x5ef2ee25 => 115
	i32 1597949149, ; 235: Xamarin.Google.ErrorProne.Annotations => 0x5f3ec4dd => 289
	i32 1600541741, ; 236: ShimSkiaSharp => 0x5f66542d => 204
	i32 1601112923, ; 237: System.Xml.Serialization => 0x5f6f0b5b => 157
	i32 1604827217, ; 238: System.Net.WebClient => 0x5fa7b851 => 76
	i32 1618516317, ; 239: System.Net.WebSockets.Client.dll => 0x6078995d => 79
	i32 1622152042, ; 240: Xamarin.AndroidX.Loader.dll => 0x60b0136a => 261
	i32 1622358360, ; 241: System.Dynamic.Runtime => 0x60b33958 => 37
	i32 1623212457, ; 242: SkiaSharp.Views.Maui.Controls => 0x60c041a9 => 208
	i32 1624863272, ; 243: Xamarin.AndroidX.ViewPager2 => 0x60d97228 => 283
	i32 1634654947, ; 244: CommunityToolkit.Maui.Core.dll => 0x616edae3 => 175
	i32 1635184631, ; 245: Xamarin.AndroidX.Emoji2.ViewsHelper => 0x6176eff7 => 245
	i32 1636350590, ; 246: Xamarin.AndroidX.CursorAdapter => 0x6188ba7e => 238
	i32 1639515021, ; 247: System.Net.Http.dll => 0x61b9038d => 64
	i32 1639986890, ; 248: System.Text.RegularExpressions => 0x61c036ca => 138
	i32 1641389582, ; 249: System.ComponentModel.EventBasedAsync.dll => 0x61d59e0e => 15
	i32 1657153582, ; 250: System.Runtime => 0x62c6282e => 116
	i32 1658241508, ; 251: Xamarin.AndroidX.Tracing.Tracing.dll => 0x62d6c1e4 => 277
	i32 1658251792, ; 252: Xamarin.Google.Android.Material.dll => 0x62d6ea10 => 286
	i32 1670060433, ; 253: Xamarin.AndroidX.ConstraintLayout => 0x638b1991 => 233
	i32 1672364457, ; 254: NetTopologySuite.IO.GeoJSON4STJ.dll => 0x63ae41a9 => 203
	i32 1675553242, ; 255: System.IO.FileSystem.DriveInfo.dll => 0x63dee9da => 48
	i32 1677501392, ; 256: System.Net.Primitives.dll => 0x63fca3d0 => 70
	i32 1678508291, ; 257: System.Net.WebSockets => 0x640c0103 => 80
	i32 1679769178, ; 258: System.Security.Cryptography => 0x641f3e5a => 126
	i32 1691477237, ; 259: System.Reflection.Metadata => 0x64d1e4f5 => 94
	i32 1696967625, ; 260: System.Security.Cryptography.Csp => 0x6525abc9 => 121
	i32 1698840827, ; 261: Xamarin.Kotlin.StdLib.Common => 0x654240fb => 297
	i32 1701541528, ; 262: System.Diagnostics.Debug.dll => 0x656b7698 => 26
	i32 1720223769, ; 263: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx => 0x66888819 => 254
	i32 1726116996, ; 264: System.Reflection.dll => 0x66e27484 => 97
	i32 1728033016, ; 265: System.Diagnostics.FileVersionInfo.dll => 0x66ffb0f8 => 28
	i32 1729485958, ; 266: Xamarin.AndroidX.CardView.dll => 0x6715dc86 => 229
	i32 1736233607, ; 267: ro/Microsoft.Maui.Controls.resources.dll => 0x677cd287 => 325
	i32 1743415430, ; 268: ca\Microsoft.Maui.Controls.resources => 0x67ea6886 => 303
	i32 1744735666, ; 269: System.Transactions.Local.dll => 0x67fe8db2 => 149
	i32 1746316138, ; 270: Mono.Android.Export => 0x6816ab6a => 169
	i32 1750313021, ; 271: Microsoft.Win32.Primitives.dll => 0x6853a83d => 4
	i32 1758240030, ; 272: System.Resources.Reader.dll => 0x68cc9d1e => 98
	i32 1763938596, ; 273: System.Diagnostics.TraceSource.dll => 0x69239124 => 33
	i32 1765942094, ; 274: System.Reflection.Extensions => 0x6942234e => 93
	i32 1766324549, ; 275: Xamarin.AndroidX.SwipeRefreshLayout => 0x6947f945 => 276
	i32 1770582343, ; 276: Microsoft.Extensions.Logging.dll => 0x6988f147 => 189
	i32 1776026572, ; 277: System.Core.dll => 0x69dc03cc => 21
	i32 1777075843, ; 278: System.Globalization.Extensions.dll => 0x69ec0683 => 41
	i32 1780572499, ; 279: Mono.Android.Runtime.dll => 0x6a216153 => 170
	i32 1782862114, ; 280: ms\Microsoft.Maui.Controls.resources => 0x6a445122 => 319
	i32 1788241197, ; 281: Xamarin.AndroidX.Fragment => 0x6a96652d => 247
	i32 1793755602, ; 282: he\Microsoft.Maui.Controls.resources => 0x6aea89d2 => 311
	i32 1808609942, ; 283: Xamarin.AndroidX.Loader => 0x6bcd3296 => 261
	i32 1813058853, ; 284: Xamarin.Kotlin.StdLib.dll => 0x6c111525 => 296
	i32 1813201214, ; 285: Xamarin.Google.Android.Material => 0x6c13413e => 286
	i32 1818569960, ; 286: Xamarin.AndroidX.Navigation.UI.dll => 0x6c652ce8 => 266
	i32 1818787751, ; 287: Microsoft.VisualBasic.Core => 0x6c687fa7 => 2
	i32 1824175904, ; 288: System.Text.Encoding.Extensions => 0x6cbab720 => 134
	i32 1824722060, ; 289: System.Runtime.Serialization.Formatters => 0x6cc30c8c => 111
	i32 1828688058, ; 290: Microsoft.Extensions.Logging.Abstractions.dll => 0x6cff90ba => 190
	i32 1839733746, ; 291: Mapsui.Nts.dll => 0x6da81bf2 => 182
	i32 1842015223, ; 292: uk/Microsoft.Maui.Controls.resources.dll => 0x6dcaebf7 => 331
	i32 1847515442, ; 293: Xamarin.Android.Glide.Annotations => 0x6e1ed932 => 216
	i32 1853025655, ; 294: sv\Microsoft.Maui.Controls.resources => 0x6e72ed77 => 328
	i32 1858542181, ; 295: System.Linq.Expressions => 0x6ec71a65 => 58
	i32 1870277092, ; 296: System.Reflection.Primitives => 0x6f7a29e4 => 95
	i32 1875935024, ; 297: fr\Microsoft.Maui.Controls.resources => 0x6fd07f30 => 310
	i32 1879696579, ; 298: System.Formats.Tar.dll => 0x7009e4c3 => 39
	i32 1885316902, ; 299: Xamarin.AndroidX.Arch.Core.Runtime.dll => 0x705fa726 => 227
	i32 1888955245, ; 300: System.Diagnostics.Contracts => 0x70972b6d => 25
	i32 1889954781, ; 301: System.Reflection.Metadata.dll => 0x70a66bdd => 94
	i32 1898237753, ; 302: System.Reflection.DispatchProxy => 0x7124cf39 => 89
	i32 1900610850, ; 303: System.Resources.ResourceManager.dll => 0x71490522 => 99
	i32 1908813208, ; 304: Xamarin.GooglePlayServices.Basement => 0x71c62d98 => 292
	i32 1910275211, ; 305: System.Collections.NonGeneric.dll => 0x71dc7c8b => 10
	i32 1939592360, ; 306: System.Private.Xml.Linq => 0x739bd4a8 => 87
	i32 1956758971, ; 307: System.Resources.Writer => 0x74a1c5bb => 100
	i32 1961813231, ; 308: Xamarin.AndroidX.Security.SecurityCrypto.dll => 0x74eee4ef => 273
	i32 1968388702, ; 309: Microsoft.Extensions.Configuration.dll => 0x75533a5e => 185
	i32 1983156543, ; 310: Xamarin.Kotlin.StdLib.Common.dll => 0x7634913f => 297
	i32 1985761444, ; 311: Xamarin.Android.Glide.GifDecoder => 0x765c50a4 => 218
	i32 2003115576, ; 312: el\Microsoft.Maui.Controls.resources => 0x77651e38 => 307
	i32 2011961780, ; 313: System.Buffers.dll => 0x77ec19b4 => 7
	i32 2019465201, ; 314: Xamarin.AndroidX.Lifecycle.ViewModel => 0x785e97f1 => 258
	i32 2025202353, ; 315: ar/Microsoft.Maui.Controls.resources.dll => 0x78b622b1 => 302
	i32 2031763787, ; 316: Xamarin.Android.Glide => 0x791a414b => 215
	i32 2045470958, ; 317: System.Private.Xml => 0x79eb68ee => 88
	i32 2055257422, ; 318: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x7a80bd4e => 253
	i32 2060060697, ; 319: System.Windows.dll => 0x7aca0819 => 154
	i32 2066184531, ; 320: de\Microsoft.Maui.Controls.resources => 0x7b277953 => 306
	i32 2070888862, ; 321: System.Diagnostics.TraceSource => 0x7b6f419e => 33
	i32 2079903147, ; 322: System.Runtime.dll => 0x7bf8cdab => 116
	i32 2090596640, ; 323: System.Numerics.Vectors => 0x7c9bf920 => 82
	i32 2127167465, ; 324: System.Console => 0x7ec9ffe9 => 20
	i32 2129483829, ; 325: Xamarin.GooglePlayServices.Base.dll => 0x7eed5835 => 291
	i32 2142473426, ; 326: System.Collections.Specialized => 0x7fb38cd2 => 11
	i32 2143790110, ; 327: System.Xml.XmlSerializer.dll => 0x7fc7a41e => 162
	i32 2146852085, ; 328: Microsoft.VisualBasic.dll => 0x7ff65cf5 => 3
	i32 2159891885, ; 329: Microsoft.Maui => 0x80bd55ad => 197
	i32 2169148018, ; 330: hu\Microsoft.Maui.Controls.resources => 0x814a9272 => 314
	i32 2181898931, ; 331: Microsoft.Extensions.Options.dll => 0x820d22b3 => 191
	i32 2192057212, ; 332: Microsoft.Extensions.Logging.Abstractions => 0x82a8237c => 190
	i32 2193016926, ; 333: System.ObjectModel.dll => 0x82b6c85e => 84
	i32 2201107256, ; 334: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x83323b38 => 301
	i32 2201231467, ; 335: System.Net.Http => 0x8334206b => 64
	i32 2207618523, ; 336: it\Microsoft.Maui.Controls.resources => 0x839595db => 316
	i32 2217644978, ; 337: Xamarin.AndroidX.VectorDrawable.Animated.dll => 0x842e93b2 => 280
	i32 2222056684, ; 338: System.Threading.Tasks.Parallel => 0x8471e4ec => 143
	i32 2244775296, ; 339: Xamarin.AndroidX.LocalBroadcastManager => 0x85cc8d80 => 262
	i32 2252106437, ; 340: System.Xml.Serialization.dll => 0x863c6ac5 => 157
	i32 2256313426, ; 341: System.Globalization.Extensions => 0x867c9c52 => 41
	i32 2265110946, ; 342: System.Security.AccessControl.dll => 0x8702d9a2 => 117
	i32 2266799131, ; 343: Microsoft.Extensions.Configuration.Abstractions => 0x871c9c1b => 186
	i32 2267999099, ; 344: Xamarin.Android.Glide.DiskLruCache.dll => 0x872eeb7b => 217
	i32 2270573516, ; 345: fr/Microsoft.Maui.Controls.resources.dll => 0x875633cc => 310
	i32 2279755925, ; 346: Xamarin.AndroidX.RecyclerView.dll => 0x87e25095 => 269
	i32 2293034957, ; 347: System.ServiceModel.Web.dll => 0x88acefcd => 131
	i32 2295906218, ; 348: System.Net.Sockets => 0x88d8bfaa => 75
	i32 2298471582, ; 349: System.Net.Mail => 0x88ffe49e => 66
	i32 2303073227, ; 350: Microsoft.Maui.Controls.Maps.dll => 0x89461bcb => 195
	i32 2303942373, ; 351: nb\Microsoft.Maui.Controls.resources => 0x89535ee5 => 320
	i32 2305521784, ; 352: System.Private.CoreLib.dll => 0x896b7878 => 172
	i32 2315684594, ; 353: Xamarin.AndroidX.Annotation.dll => 0x8a068af2 => 221
	i32 2320631194, ; 354: System.Threading.Tasks.Parallel.dll => 0x8a52059a => 143
	i32 2327893114, ; 355: ExCSS.dll => 0x8ac0d47a => 177
	i32 2340441535, ; 356: System.Runtime.InteropServices.RuntimeInformation.dll => 0x8b804dbf => 106
	i32 2344264397, ; 357: System.ValueTuple => 0x8bbaa2cd => 151
	i32 2353062107, ; 358: System.Net.Primitives => 0x8c40e0db => 70
	i32 2364201794, ; 359: SkiaSharp.Views.Maui.Core => 0x8ceadb42 => 210
	i32 2368005991, ; 360: System.Xml.ReaderWriter.dll => 0x8d24e767 => 156
	i32 2371007202, ; 361: Microsoft.Extensions.Configuration => 0x8d52b2e2 => 185
	i32 2378619854, ; 362: System.Security.Cryptography.Csp.dll => 0x8dc6dbce => 121
	i32 2383496789, ; 363: System.Security.Principal.Windows.dll => 0x8e114655 => 127
	i32 2395872292, ; 364: id\Microsoft.Maui.Controls.resources => 0x8ece1c24 => 315
	i32 2401565422, ; 365: System.Web.HttpUtility => 0x8f24faee => 152
	i32 2403452196, ; 366: Xamarin.AndroidX.Emoji2.dll => 0x8f41c524 => 244
	i32 2421380589, ; 367: System.Threading.Tasks.Dataflow => 0x905355ed => 141
	i32 2423080555, ; 368: Xamarin.AndroidX.Collection.Ktx.dll => 0x906d466b => 231
	i32 2427813419, ; 369: hi\Microsoft.Maui.Controls.resources => 0x90b57e2b => 312
	i32 2435356389, ; 370: System.Console.dll => 0x912896e5 => 20
	i32 2435904999, ; 371: System.ComponentModel.DataAnnotations.dll => 0x9130f5e7 => 14
	i32 2454642406, ; 372: System.Text.Encoding.dll => 0x924edee6 => 135
	i32 2458678730, ; 373: System.Net.Sockets.dll => 0x928c75ca => 75
	i32 2459001652, ; 374: System.Linq.Parallel.dll => 0x92916334 => 59
	i32 2465532216, ; 375: Xamarin.AndroidX.ConstraintLayout.Core.dll => 0x92f50938 => 234
	i32 2471841756, ; 376: netstandard.dll => 0x93554fdc => 167
	i32 2475788418, ; 377: Java.Interop.dll => 0x93918882 => 168
	i32 2480646305, ; 378: Microsoft.Maui.Controls => 0x93dba8a1 => 194
	i32 2483903535, ; 379: System.ComponentModel.EventBasedAsync => 0x940d5c2f => 15
	i32 2484371297, ; 380: System.Net.ServicePoint => 0x94147f61 => 74
	i32 2490993605, ; 381: System.AppContext.dll => 0x94798bc5 => 6
	i32 2501346920, ; 382: System.Data.DataSetExtensions => 0x95178668 => 23
	i32 2505896520, ; 383: Xamarin.AndroidX.Lifecycle.Runtime.dll => 0x955cf248 => 256
	i32 2521915375, ; 384: SkiaSharp.Views.Maui.Controls.Compatibility => 0x96515fef => 209
	i32 2522472828, ; 385: Xamarin.Android.Glide.dll => 0x9659e17c => 215
	i32 2523023297, ; 386: Svg.Custom.dll => 0x966247c1 => 211
	i32 2538310050, ; 387: System.Reflection.Emit.Lightweight.dll => 0x974b89a2 => 91
	i32 2550873716, ; 388: hr\Microsoft.Maui.Controls.resources => 0x980b3e74 => 313
	i32 2562349572, ; 389: Microsoft.CSharp => 0x98ba5a04 => 1
	i32 2570120770, ; 390: System.Text.Encodings.Web => 0x9930ee42 => 136
	i32 2577414832, ; 391: Mapsui.Nts => 0x99a03ab0 => 182
	i32 2581783588, ; 392: Xamarin.AndroidX.Lifecycle.Runtime.Ktx => 0x99e2e424 => 257
	i32 2581819634, ; 393: Xamarin.AndroidX.VectorDrawable.dll => 0x99e370f2 => 279
	i32 2585220780, ; 394: System.Text.Encoding.Extensions.dll => 0x9a1756ac => 134
	i32 2585805581, ; 395: System.Net.Ping => 0x9a20430d => 69
	i32 2589602615, ; 396: System.Threading.ThreadPool => 0x9a5a3337 => 146
	i32 2593496499, ; 397: pl\Microsoft.Maui.Controls.resources => 0x9a959db3 => 322
	i32 2602257211, ; 398: Svg.Model.dll => 0x9b1b4b3b => 212
	i32 2605712449, ; 399: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x9b500441 => 301
	i32 2609324236, ; 400: Svg.Custom => 0x9b8720cc => 211
	i32 2615233544, ; 401: Xamarin.AndroidX.Fragment.Ktx => 0x9be14c08 => 248
	i32 2617129537, ; 402: System.Private.Xml.dll => 0x9bfe3a41 => 88
	i32 2618712057, ; 403: System.Reflection.TypeExtensions.dll => 0x9c165ff9 => 96
	i32 2620871830, ; 404: Xamarin.AndroidX.CursorAdapter.dll => 0x9c375496 => 238
	i32 2624644809, ; 405: Xamarin.AndroidX.DynamicAnimation => 0x9c70e6c9 => 243
	i32 2625339995, ; 406: SkiaSharp.Views.Maui.Core.dll => 0x9c7b825b => 210
	i32 2626831493, ; 407: ja\Microsoft.Maui.Controls.resources => 0x9c924485 => 317
	i32 2627185994, ; 408: System.Diagnostics.TextWriterTraceListener.dll => 0x9c97ad4a => 31
	i32 2629843544, ; 409: System.IO.Compression.ZipFile.dll => 0x9cc03a58 => 45
	i32 2633051222, ; 410: Xamarin.AndroidX.Lifecycle.LiveData => 0x9cf12c56 => 252
	i32 2663391936, ; 411: Xamarin.Android.Glide.DiskLruCache => 0x9ec022c0 => 217
	i32 2663698177, ; 412: System.Runtime.Loader => 0x9ec4cf01 => 109
	i32 2664396074, ; 413: System.Xml.XDocument.dll => 0x9ecf752a => 158
	i32 2665622720, ; 414: System.Drawing.Primitives => 0x9ee22cc0 => 35
	i32 2676780864, ; 415: System.Data.Common.dll => 0x9f8c6f40 => 22
	i32 2686887180, ; 416: System.Runtime.Serialization.Xml.dll => 0xa026a50c => 114
	i32 2693849962, ; 417: System.IO.dll => 0xa090e36a => 57
	i32 2701096212, ; 418: Xamarin.AndroidX.Tracing.Tracing => 0xa0ff7514 => 277
	i32 2715334215, ; 419: System.Threading.Tasks.dll => 0xa1d8b647 => 144
	i32 2717744543, ; 420: System.Security.Claims => 0xa1fd7d9f => 118
	i32 2719963679, ; 421: System.Security.Cryptography.Cng.dll => 0xa21f5a1f => 120
	i32 2724373263, ; 422: System.Runtime.Numerics.dll => 0xa262a30f => 110
	i32 2732626843, ; 423: Xamarin.AndroidX.Activity => 0xa2e0939b => 219
	i32 2735172069, ; 424: System.Threading.Channels => 0xa30769e5 => 139
	i32 2737747696, ; 425: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0xa32eb6f0 => 225
	i32 2740948882, ; 426: System.IO.Pipes.AccessControl => 0xa35f8f92 => 54
	i32 2748088231, ; 427: System.Runtime.InteropServices.JavaScript => 0xa3cc7fa7 => 105
	i32 2752995522, ; 428: pt-BR\Microsoft.Maui.Controls.resources => 0xa41760c2 => 323
	i32 2756874198, ; 429: NetTopologySuite.IO.GeoJSON4STJ => 0xa4528fd6 => 203
	i32 2758225723, ; 430: Microsoft.Maui.Controls.Xaml => 0xa4672f3b => 196
	i32 2764765095, ; 431: Microsoft.Maui.dll => 0xa4caf7a7 => 197
	i32 2765824710, ; 432: System.Text.Encoding.CodePages.dll => 0xa4db22c6 => 133
	i32 2770495804, ; 433: Xamarin.Jetbrains.Annotations.dll => 0xa522693c => 295
	i32 2778768386, ; 434: Xamarin.AndroidX.ViewPager.dll => 0xa5a0a402 => 282
	i32 2779977773, ; 435: Xamarin.AndroidX.ResourceInspection.Annotation.dll => 0xa5b3182d => 270
	i32 2785988530, ; 436: th\Microsoft.Maui.Controls.resources => 0xa60ecfb2 => 329
	i32 2788224221, ; 437: Xamarin.AndroidX.Fragment.Ktx.dll => 0xa630ecdd => 248
	i32 2795602088, ; 438: SkiaSharp.Views.Android.dll => 0xa6a180a8 => 207
	i32 2801831435, ; 439: Microsoft.Maui.Graphics => 0xa7008e0b => 199
	i32 2803228030, ; 440: System.Xml.XPath.XDocument.dll => 0xa715dd7e => 159
	i32 2806116107, ; 441: es/Microsoft.Maui.Controls.resources.dll => 0xa741ef0b => 308
	i32 2810250172, ; 442: Xamarin.AndroidX.CoordinatorLayout.dll => 0xa78103bc => 235
	i32 2819470561, ; 443: System.Xml.dll => 0xa80db4e1 => 163
	i32 2821205001, ; 444: System.ServiceProcess.dll => 0xa8282c09 => 132
	i32 2821294376, ; 445: Xamarin.AndroidX.ResourceInspection.Annotation => 0xa8298928 => 270
	i32 2824502124, ; 446: System.Xml.XmlDocument => 0xa85a7b6c => 161
	i32 2831556043, ; 447: nl/Microsoft.Maui.Controls.resources.dll => 0xa8c61dcb => 321
	i32 2838993487, ; 448: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx.dll => 0xa9379a4f => 259
	i32 2847418871, ; 449: Xamarin.GooglePlayServices.Base => 0xa9b829f7 => 291
	i32 2849599387, ; 450: System.Threading.Overlapped.dll => 0xa9d96f9b => 140
	i32 2853208004, ; 451: Xamarin.AndroidX.ViewPager => 0xaa107fc4 => 282
	i32 2855708567, ; 452: Xamarin.AndroidX.Transition => 0xaa36a797 => 278
	i32 2861098320, ; 453: Mono.Android.Export.dll => 0xaa88e550 => 169
	i32 2861189240, ; 454: Microsoft.Maui.Essentials => 0xaa8a4878 => 198
	i32 2868488919, ; 455: CommunityToolkit.Maui.Core => 0xaaf9aad7 => 175
	i32 2870099610, ; 456: Xamarin.AndroidX.Activity.Ktx.dll => 0xab123e9a => 220
	i32 2875164099, ; 457: Jsr305Binding.dll => 0xab5f85c3 => 287
	i32 2875220617, ; 458: System.Globalization.Calendars.dll => 0xab606289 => 40
	i32 2884993177, ; 459: Xamarin.AndroidX.ExifInterface => 0xabf58099 => 246
	i32 2887636118, ; 460: System.Net.dll => 0xac1dd496 => 81
	i32 2899753641, ; 461: System.IO.UnmanagedMemoryStream => 0xacd6baa9 => 56
	i32 2900621748, ; 462: System.Dynamic.Runtime.dll => 0xace3f9b4 => 37
	i32 2901442782, ; 463: System.Reflection => 0xacf080de => 97
	i32 2905242038, ; 464: mscorlib.dll => 0xad2a79b6 => 166
	i32 2909740682, ; 465: System.Private.CoreLib => 0xad6f1e8a => 172
	i32 2912489636, ; 466: SkiaSharp.Views.Android => 0xad9910a4 => 207
	i32 2916838712, ; 467: Xamarin.AndroidX.ViewPager2.dll => 0xaddb6d38 => 283
	i32 2919462931, ; 468: System.Numerics.Vectors.dll => 0xae037813 => 82
	i32 2921128767, ; 469: Xamarin.AndroidX.Annotation.Experimental.dll => 0xae1ce33f => 222
	i32 2936416060, ; 470: System.Resources.Reader => 0xaf06273c => 98
	i32 2940926066, ; 471: System.Diagnostics.StackTrace.dll => 0xaf4af872 => 30
	i32 2942453041, ; 472: System.Xml.XPath.XDocument => 0xaf624531 => 159
	i32 2959614098, ; 473: System.ComponentModel.dll => 0xb0682092 => 18
	i32 2968338931, ; 474: System.Security.Principal.Windows => 0xb0ed41f3 => 127
	i32 2972252294, ; 475: System.Security.Cryptography.Algorithms.dll => 0xb128f886 => 119
	i32 2978675010, ; 476: Xamarin.AndroidX.DrawerLayout => 0xb18af942 => 242
	i32 2987532451, ; 477: Xamarin.AndroidX.Security.SecurityCrypto => 0xb21220a3 => 273
	i32 2996846495, ; 478: Xamarin.AndroidX.Lifecycle.Process.dll => 0xb2a03f9f => 255
	i32 3016983068, ; 479: Xamarin.AndroidX.Startup.StartupRuntime => 0xb3d3821c => 275
	i32 3017076677, ; 480: Xamarin.GooglePlayServices.Maps => 0xb3d4efc5 => 293
	i32 3023353419, ; 481: WindowsBase.dll => 0xb434b64b => 165
	i32 3024354802, ; 482: Xamarin.AndroidX.Legacy.Support.Core.Utils => 0xb443fdf2 => 250
	i32 3038032645, ; 483: _Microsoft.Android.Resource.Designer.dll => 0xb514b305 => 336
	i32 3056245963, ; 484: Xamarin.AndroidX.SavedState.SavedState.Ktx => 0xb62a9ccb => 272
	i32 3057625584, ; 485: Xamarin.AndroidX.Navigation.Common => 0xb63fa9f0 => 263
	i32 3058099980, ; 486: Xamarin.GooglePlayServices.Tasks => 0xb646e70c => 294
	i32 3059408633, ; 487: Mono.Android.Runtime => 0xb65adef9 => 170
	i32 3059793426, ; 488: System.ComponentModel.Primitives => 0xb660be12 => 16
	i32 3075834255, ; 489: System.Threading.Tasks => 0xb755818f => 144
	i32 3077302341, ; 490: hu/Microsoft.Maui.Controls.resources.dll => 0xb76be845 => 314
	i32 3090735792, ; 491: System.Security.Cryptography.X509Certificates.dll => 0xb838e2b0 => 125
	i32 3099732863, ; 492: System.Security.Claims.dll => 0xb8c22b7f => 118
	i32 3103600923, ; 493: System.Formats.Asn1 => 0xb8fd311b => 38
	i32 3111772706, ; 494: System.Runtime.Serialization => 0xb979e222 => 115
	i32 3121463068, ; 495: System.IO.FileSystem.AccessControl.dll => 0xba0dbf1c => 47
	i32 3124832203, ; 496: System.Threading.Tasks.Extensions => 0xba4127cb => 142
	i32 3132293585, ; 497: System.Security.AccessControl => 0xbab301d1 => 117
	i32 3134694676, ; 498: ShimSkiaSharp.dll => 0xbad7a514 => 204
	i32 3147165239, ; 499: System.Diagnostics.Tracing.dll => 0xbb95ee37 => 34
	i32 3148237826, ; 500: GoogleGson.dll => 0xbba64c02 => 178
	i32 3159123045, ; 501: System.Reflection.Primitives.dll => 0xbc4c6465 => 95
	i32 3160747431, ; 502: System.IO.MemoryMappedFiles => 0xbc652da7 => 53
	i32 3178803400, ; 503: Xamarin.AndroidX.Navigation.Fragment.dll => 0xbd78b0c8 => 264
	i32 3192346100, ; 504: System.Security.SecureString => 0xbe4755f4 => 129
	i32 3193515020, ; 505: System.Web => 0xbe592c0c => 153
	i32 3204380047, ; 506: System.Data.dll => 0xbefef58f => 24
	i32 3209718065, ; 507: System.Xml.XmlDocument.dll => 0xbf506931 => 161
	i32 3211777861, ; 508: Xamarin.AndroidX.DocumentFile => 0xbf6fd745 => 241
	i32 3220365878, ; 509: System.Threading => 0xbff2e236 => 148
	i32 3226221578, ; 510: System.Runtime.Handles.dll => 0xc04c3c0a => 104
	i32 3230466174, ; 511: Xamarin.GooglePlayServices.Basement.dll => 0xc08d007e => 292
	i32 3251039220, ; 512: System.Reflection.DispatchProxy.dll => 0xc1c6ebf4 => 89
	i32 3258312781, ; 513: Xamarin.AndroidX.CardView => 0xc235e84d => 229
	i32 3265493905, ; 514: System.Linq.Queryable.dll => 0xc2a37b91 => 60
	i32 3265893370, ; 515: System.Threading.Tasks.Extensions.dll => 0xc2a993fa => 142
	i32 3277815716, ; 516: System.Resources.Writer.dll => 0xc35f7fa4 => 100
	i32 3278552754, ; 517: Mapsui => 0xc36abeb2 => 180
	i32 3279906254, ; 518: Microsoft.Win32.Registry.dll => 0xc37f65ce => 5
	i32 3280506390, ; 519: System.ComponentModel.Annotations.dll => 0xc3888e16 => 13
	i32 3290767353, ; 520: System.Security.Cryptography.Encoding => 0xc4251ff9 => 122
	i32 3299363146, ; 521: System.Text.Encoding => 0xc4a8494a => 135
	i32 3303498502, ; 522: System.Diagnostics.FileVersionInfo => 0xc4e76306 => 28
	i32 3305363605, ; 523: fi\Microsoft.Maui.Controls.resources => 0xc503d895 => 309
	i32 3316684772, ; 524: System.Net.Requests.dll => 0xc5b097e4 => 72
	i32 3317135071, ; 525: Xamarin.AndroidX.CustomView.dll => 0xc5b776df => 239
	i32 3317144872, ; 526: System.Data => 0xc5b79d28 => 24
	i32 3340387945, ; 527: SkiaSharp => 0xc71a4669 => 205
	i32 3340431453, ; 528: Xamarin.AndroidX.Arch.Core.Runtime => 0xc71af05d => 227
	i32 3345895724, ; 529: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller.dll => 0xc76e512c => 268
	i32 3346324047, ; 530: Xamarin.AndroidX.Navigation.Runtime => 0xc774da4f => 265
	i32 3357674450, ; 531: ru\Microsoft.Maui.Controls.resources => 0xc8220bd2 => 326
	i32 3358260929, ; 532: System.Text.Json => 0xc82afec1 => 137
	i32 3362336904, ; 533: Xamarin.AndroidX.Activity.Ktx => 0xc8693088 => 220
	i32 3362522851, ; 534: Xamarin.AndroidX.Core => 0xc86c06e3 => 236
	i32 3366347497, ; 535: Java.Interop => 0xc8a662e9 => 168
	i32 3374999561, ; 536: Xamarin.AndroidX.RecyclerView => 0xc92a6809 => 269
	i32 3381016424, ; 537: da\Microsoft.Maui.Controls.resources => 0xc9863768 => 305
	i32 3395150330, ; 538: System.Runtime.CompilerServices.Unsafe.dll => 0xca5de1fa => 101
	i32 3403906625, ; 539: System.Security.Cryptography.OpenSsl.dll => 0xcae37e41 => 123
	i32 3405233483, ; 540: Xamarin.AndroidX.CustomView.PoolingContainer => 0xcaf7bd4b => 240
	i32 3428513518, ; 541: Microsoft.Extensions.DependencyInjection.dll => 0xcc5af6ee => 187
	i32 3429136800, ; 542: System.Xml => 0xcc6479a0 => 163
	i32 3430777524, ; 543: netstandard => 0xcc7d82b4 => 167
	i32 3441283291, ; 544: Xamarin.AndroidX.DynamicAnimation.dll => 0xcd1dd0db => 243
	i32 3445260447, ; 545: System.Formats.Tar => 0xcd5a809f => 39
	i32 3452344032, ; 546: Microsoft.Maui.Controls.Compatibility.dll => 0xcdc696e0 => 193
	i32 3459815001, ; 547: Mapsui.Rendering.Skia => 0xce389659 => 183
	i32 3463511458, ; 548: hr/Microsoft.Maui.Controls.resources.dll => 0xce70fda2 => 313
	i32 3466574376, ; 549: SkiaSharp.Views.Maui.Controls.Compatibility.dll => 0xce9fba28 => 209
	i32 3471940407, ; 550: System.ComponentModel.TypeConverter.dll => 0xcef19b37 => 17
	i32 3473156932, ; 551: SkiaSharp.Views.Maui.Controls.dll => 0xcf042b44 => 208
	i32 3476120550, ; 552: Mono.Android => 0xcf3163e6 => 171
	i32 3479583265, ; 553: ru/Microsoft.Maui.Controls.resources.dll => 0xcf663a21 => 326
	i32 3484440000, ; 554: ro\Microsoft.Maui.Controls.resources => 0xcfb055c0 => 325
	i32 3485117614, ; 555: System.Text.Json.dll => 0xcfbaacae => 137
	i32 3486566296, ; 556: System.Transactions => 0xcfd0c798 => 150
	i32 3493954962, ; 557: Xamarin.AndroidX.Concurrent.Futures.dll => 0xd0418592 => 232
	i32 3500773090, ; 558: Microsoft.Maui.Controls.Maps => 0xd0a98ee2 => 195
	i32 3509114376, ; 559: System.Xml.Linq => 0xd128d608 => 155
	i32 3515174580, ; 560: System.Security.dll => 0xd1854eb4 => 130
	i32 3530912306, ; 561: System.Configuration => 0xd2757232 => 19
	i32 3539954161, ; 562: System.Net.HttpListener => 0xd2ff69f1 => 65
	i32 3560100363, ; 563: System.Threading.Timer => 0xd432d20b => 147
	i32 3570554715, ; 564: System.IO.FileSystem.AccessControl => 0xd4d2575b => 47
	i32 3580758918, ; 565: zh-HK\Microsoft.Maui.Controls.resources => 0xd56e0b86 => 333
	i32 3597029428, ; 566: Xamarin.Android.Glide.GifDecoder.dll => 0xd6665034 => 218
	i32 3598340787, ; 567: System.Net.WebSockets.Client => 0xd67a52b3 => 79
	i32 3608519521, ; 568: System.Linq.dll => 0xd715a361 => 61
	i32 3624195450, ; 569: System.Runtime.InteropServices.RuntimeInformation => 0xd804d57a => 106
	i32 3627220390, ; 570: Xamarin.AndroidX.Print.dll => 0xd832fda6 => 267
	i32 3633644679, ; 571: Xamarin.AndroidX.Annotation.Experimental => 0xd8950487 => 222
	i32 3638274909, ; 572: System.IO.FileSystem.Primitives.dll => 0xd8dbab5d => 49
	i32 3641597786, ; 573: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0xd90e5f5a => 253
	i32 3643446276, ; 574: tr\Microsoft.Maui.Controls.resources => 0xd92a9404 => 330
	i32 3643854240, ; 575: Xamarin.AndroidX.Navigation.Fragment => 0xd930cda0 => 264
	i32 3645089577, ; 576: System.ComponentModel.DataAnnotations => 0xd943a729 => 14
	i32 3657292374, ; 577: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd9fdda56 => 186
	i32 3660523487, ; 578: System.Net.NetworkInformation => 0xda2f27df => 68
	i32 3672681054, ; 579: Mono.Android.dll => 0xdae8aa5e => 171
	i32 3682565725, ; 580: Xamarin.AndroidX.Browser => 0xdb7f7e5d => 228
	i32 3684561358, ; 581: Xamarin.AndroidX.Concurrent.Futures => 0xdb9df1ce => 232
	i32 3697841164, ; 582: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xdc68940c => 335
	i32 3700866549, ; 583: System.Net.WebProxy.dll => 0xdc96bdf5 => 78
	i32 3706696989, ; 584: Xamarin.AndroidX.Core.Core.Ktx.dll => 0xdcefb51d => 237
	i32 3712156464, ; 585: Mapsui.UI.Maui.dll => 0xdd430330 => 181
	i32 3716563718, ; 586: System.Runtime.Intrinsics => 0xdd864306 => 108
	i32 3718780102, ; 587: Xamarin.AndroidX.Annotation => 0xdda814c6 => 221
	i32 3724971120, ; 588: Xamarin.AndroidX.Navigation.Common.dll => 0xde068c70 => 263
	i32 3732100267, ; 589: System.Net.NameResolution => 0xde7354ab => 67
	i32 3737834244, ; 590: System.Net.Http.Json.dll => 0xdecad304 => 63
	i32 3748608112, ; 591: System.Diagnostics.DiagnosticSource => 0xdf6f3870 => 27
	i32 3751444290, ; 592: System.Xml.XPath => 0xdf9a7f42 => 160
	i32 3786282454, ; 593: Xamarin.AndroidX.Collection => 0xe1ae15d6 => 230
	i32 3792276235, ; 594: System.Collections.NonGeneric => 0xe2098b0b => 10
	i32 3792835768, ; 595: HarfBuzzSharp => 0xe21214b8 => 179
	i32 3798102808, ; 596: BruTile => 0xe2627318 => 173
	i32 3800979733, ; 597: Microsoft.Maui.Controls.Compatibility => 0xe28e5915 => 193
	i32 3802395368, ; 598: System.Collections.Specialized.dll => 0xe2a3f2e8 => 11
	i32 3817368567, ; 599: CommunityToolkit.Maui.dll => 0xe3886bf7 => 174
	i32 3819260425, ; 600: System.Net.WebProxy => 0xe3a54a09 => 78
	i32 3823082795, ; 601: System.Security.Cryptography.dll => 0xe3df9d2b => 126
	i32 3829621856, ; 602: System.Numerics.dll => 0xe4436460 => 83
	i32 3841636137, ; 603: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xe4fab729 => 188
	i32 3844307129, ; 604: System.Net.Mail.dll => 0xe52378b9 => 66
	i32 3849253459, ; 605: System.Runtime.InteropServices.dll => 0xe56ef253 => 107
	i32 3870376305, ; 606: System.Net.HttpListener.dll => 0xe6b14171 => 65
	i32 3873536506, ; 607: System.Security.Principal => 0xe6e179fa => 128
	i32 3875112723, ; 608: System.Security.Cryptography.Encoding.dll => 0xe6f98713 => 122
	i32 3885497537, ; 609: System.Net.WebHeaderCollection.dll => 0xe797fcc1 => 77
	i32 3885922214, ; 610: Xamarin.AndroidX.Transition.dll => 0xe79e77a6 => 278
	i32 3888767677, ; 611: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller => 0xe7c9e2bd => 268
	i32 3889960447, ; 612: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xe7dc15ff => 334
	i32 3896106733, ; 613: System.Collections.Concurrent.dll => 0xe839deed => 8
	i32 3896760992, ; 614: Xamarin.AndroidX.Core.dll => 0xe843daa0 => 236
	i32 3901907137, ; 615: Microsoft.VisualBasic.Core.dll => 0xe89260c1 => 2
	i32 3920810846, ; 616: System.IO.Compression.FileSystem.dll => 0xe9b2d35e => 44
	i32 3921031405, ; 617: Xamarin.AndroidX.VersionedParcelable.dll => 0xe9b630ed => 281
	i32 3928044579, ; 618: System.Xml.ReaderWriter => 0xea213423 => 156
	i32 3930554604, ; 619: System.Security.Principal.dll => 0xea4780ec => 128
	i32 3931092270, ; 620: Xamarin.AndroidX.Navigation.UI => 0xea4fb52e => 266
	i32 3934069706, ; 621: Topten.RichTextKit.dll => 0xea7d23ca => 214
	i32 3945713374, ; 622: System.Data.DataSetExtensions.dll => 0xeb2ecede => 23
	i32 3952289091, ; 623: NetTopologySuite.Features.dll => 0xeb932543 => 202
	i32 3953583589, ; 624: Svg.Skia => 0xeba6e5e5 => 213
	i32 3953953790, ; 625: System.Text.Encoding.CodePages => 0xebac8bfe => 133
	i32 3955647286, ; 626: Xamarin.AndroidX.AppCompat.dll => 0xebc66336 => 224
	i32 3959773229, ; 627: Xamarin.AndroidX.Lifecycle.Process => 0xec05582d => 255
	i32 3970018735, ; 628: Xamarin.GooglePlayServices.Tasks.dll => 0xeca1adaf => 294
	i32 3980434154, ; 629: th/Microsoft.Maui.Controls.resources.dll => 0xed409aea => 329
	i32 3987592930, ; 630: he/Microsoft.Maui.Controls.resources.dll => 0xedadd6e2 => 311
	i32 4003436829, ; 631: System.Diagnostics.Process.dll => 0xee9f991d => 29
	i32 4003906742, ; 632: HarfBuzzSharp.dll => 0xeea6c4b6 => 179
	i32 4013003792, ; 633: BruTile.dll => 0xef319410 => 173
	i32 4015948917, ; 634: Xamarin.AndroidX.Annotation.Jvm.dll => 0xef5e8475 => 223
	i32 4022681963, ; 635: Mapsui.Tiling => 0xefc5416b => 184
	i32 4025784931, ; 636: System.Memory => 0xeff49a63 => 62
	i32 4046471985, ; 637: Microsoft.Maui.Controls.Xaml.dll => 0xf1304331 => 196
	i32 4054681211, ; 638: System.Reflection.Emit.ILGeneration => 0xf1ad867b => 90
	i32 4066802364, ; 639: SkiaSharp.HarfBuzz => 0xf2667abc => 206
	i32 4068434129, ; 640: System.Private.Xml.Linq.dll => 0xf27f60d1 => 87
	i32 4073602200, ; 641: System.Threading.dll => 0xf2ce3c98 => 148
	i32 4094352644, ; 642: Microsoft.Maui.Essentials.dll => 0xf40add04 => 198
	i32 4099507663, ; 643: System.Drawing.dll => 0xf45985cf => 36
	i32 4100113165, ; 644: System.Private.Uri => 0xf462c30d => 86
	i32 4101593132, ; 645: Xamarin.AndroidX.Emoji2 => 0xf479582c => 244
	i32 4102112229, ; 646: pt/Microsoft.Maui.Controls.resources.dll => 0xf48143e5 => 324
	i32 4116972982, ; 647: DoAnCSharp => 0xf56405b6 => 0
	i32 4125707920, ; 648: ms/Microsoft.Maui.Controls.resources.dll => 0xf5e94e90 => 319
	i32 4126470640, ; 649: Microsoft.Extensions.DependencyInjection => 0xf5f4f1f0 => 187
	i32 4127667938, ; 650: System.IO.FileSystem.Watcher => 0xf60736e2 => 50
	i32 4130442656, ; 651: System.AppContext => 0xf6318da0 => 6
	i32 4144557198, ; 652: NetTopologySuite.Features => 0xf708ec8e => 202
	i32 4147896353, ; 653: System.Reflection.Emit.ILGeneration.dll => 0xf73be021 => 90
	i32 4150914736, ; 654: uk\Microsoft.Maui.Controls.resources => 0xf769eeb0 => 331
	i32 4151237749, ; 655: System.Core => 0xf76edc75 => 21
	i32 4159265925, ; 656: System.Xml.XmlSerializer => 0xf7e95c85 => 162
	i32 4161255271, ; 657: System.Reflection.TypeExtensions => 0xf807b767 => 96
	i32 4164802419, ; 658: System.IO.FileSystem.Watcher.dll => 0xf83dd773 => 50
	i32 4181436372, ; 659: System.Runtime.Serialization.Primitives => 0xf93ba7d4 => 113
	i32 4182413190, ; 660: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0xf94a8f86 => 260
	i32 4185676441, ; 661: System.Security => 0xf97c5a99 => 130
	i32 4190991637, ; 662: Microsoft.Maui.Maps.dll => 0xf9cd7515 => 200
	i32 4196529839, ; 663: System.Net.WebClient.dll => 0xfa21f6af => 76
	i32 4213026141, ; 664: System.Diagnostics.DiagnosticSource.dll => 0xfb1dad5d => 27
	i32 4256097574, ; 665: Xamarin.AndroidX.Core.Core.Ktx => 0xfdaee526 => 237
	i32 4258378803, ; 666: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx => 0xfdd1b433 => 259
	i32 4260525087, ; 667: System.Buffers => 0xfdf2741f => 7
	i32 4271975918, ; 668: Microsoft.Maui.Controls.dll => 0xfea12dee => 194
	i32 4274623895, ; 669: CommunityToolkit.Mvvm.dll => 0xfec99597 => 176
	i32 4274976490, ; 670: System.Runtime.Numerics => 0xfecef6ea => 110
	i32 4278134329, ; 671: Xamarin.GooglePlayServices.Maps.dll => 0xfeff2639 => 293
	i32 4292120959, ; 672: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xffd4917f => 260
	i32 4294763496 ; 673: Xamarin.AndroidX.ExifInterface.dll => 0xfffce3e8 => 246
], align 4

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [674 x i32] [
	i32 68, ; 0
	i32 67, ; 1
	i32 108, ; 2
	i32 256, ; 3
	i32 290, ; 4
	i32 48, ; 5
	i32 80, ; 6
	i32 145, ; 7
	i32 30, ; 8
	i32 335, ; 9
	i32 124, ; 10
	i32 199, ; 11
	i32 102, ; 12
	i32 274, ; 13
	i32 107, ; 14
	i32 274, ; 15
	i32 139, ; 16
	i32 298, ; 17
	i32 77, ; 18
	i32 213, ; 19
	i32 124, ; 20
	i32 13, ; 21
	i32 0, ; 22
	i32 230, ; 23
	i32 132, ; 24
	i32 276, ; 25
	i32 151, ; 26
	i32 332, ; 27
	i32 333, ; 28
	i32 18, ; 29
	i32 228, ; 30
	i32 26, ; 31
	i32 250, ; 32
	i32 1, ; 33
	i32 59, ; 34
	i32 42, ; 35
	i32 91, ; 36
	i32 233, ; 37
	i32 147, ; 38
	i32 252, ; 39
	i32 249, ; 40
	i32 304, ; 41
	i32 54, ; 42
	i32 181, ; 43
	i32 69, ; 44
	i32 332, ; 45
	i32 219, ; 46
	i32 83, ; 47
	i32 317, ; 48
	i32 251, ; 49
	i32 316, ; 50
	i32 131, ; 51
	i32 55, ; 52
	i32 149, ; 53
	i32 74, ; 54
	i32 145, ; 55
	i32 62, ; 56
	i32 146, ; 57
	i32 336, ; 58
	i32 165, ; 59
	i32 328, ; 60
	i32 234, ; 61
	i32 12, ; 62
	i32 247, ; 63
	i32 125, ; 64
	i32 152, ; 65
	i32 113, ; 66
	i32 177, ; 67
	i32 166, ; 68
	i32 164, ; 69
	i32 212, ; 70
	i32 249, ; 71
	i32 262, ; 72
	i32 84, ; 73
	i32 315, ; 74
	i32 309, ; 75
	i32 192, ; 76
	i32 205, ; 77
	i32 150, ; 78
	i32 298, ; 79
	i32 60, ; 80
	i32 189, ; 81
	i32 51, ; 82
	i32 103, ; 83
	i32 114, ; 84
	i32 40, ; 85
	i32 287, ; 86
	i32 285, ; 87
	i32 120, ; 88
	i32 323, ; 89
	i32 174, ; 90
	i32 52, ; 91
	i32 44, ; 92
	i32 119, ; 93
	i32 239, ; 94
	i32 321, ; 95
	i32 245, ; 96
	i32 81, ; 97
	i32 136, ; 98
	i32 281, ; 99
	i32 226, ; 100
	i32 8, ; 101
	i32 183, ; 102
	i32 73, ; 103
	i32 303, ; 104
	i32 155, ; 105
	i32 300, ; 106
	i32 154, ; 107
	i32 92, ; 108
	i32 295, ; 109
	i32 45, ; 110
	i32 318, ; 111
	i32 306, ; 112
	i32 299, ; 113
	i32 109, ; 114
	i32 129, ; 115
	i32 25, ; 116
	i32 216, ; 117
	i32 72, ; 118
	i32 55, ; 119
	i32 46, ; 120
	i32 327, ; 121
	i32 206, ; 122
	i32 191, ; 123
	i32 240, ; 124
	i32 22, ; 125
	i32 254, ; 126
	i32 86, ; 127
	i32 43, ; 128
	i32 160, ; 129
	i32 71, ; 130
	i32 267, ; 131
	i32 3, ; 132
	i32 42, ; 133
	i32 63, ; 134
	i32 201, ; 135
	i32 16, ; 136
	i32 200, ; 137
	i32 53, ; 138
	i32 330, ; 139
	i32 290, ; 140
	i32 105, ; 141
	i32 299, ; 142
	i32 288, ; 143
	i32 251, ; 144
	i32 34, ; 145
	i32 158, ; 146
	i32 85, ; 147
	i32 32, ; 148
	i32 12, ; 149
	i32 51, ; 150
	i32 56, ; 151
	i32 271, ; 152
	i32 36, ; 153
	i32 188, ; 154
	i32 305, ; 155
	i32 289, ; 156
	i32 224, ; 157
	i32 35, ; 158
	i32 58, ; 159
	i32 258, ; 160
	i32 178, ; 161
	i32 17, ; 162
	i32 296, ; 163
	i32 164, ; 164
	i32 318, ; 165
	i32 257, ; 166
	i32 284, ; 167
	i32 324, ; 168
	i32 153, ; 169
	i32 280, ; 170
	i32 265, ; 171
	i32 322, ; 172
	i32 226, ; 173
	i32 29, ; 174
	i32 176, ; 175
	i32 52, ; 176
	i32 320, ; 177
	i32 285, ; 178
	i32 5, ; 179
	i32 304, ; 180
	i32 275, ; 181
	i32 279, ; 182
	i32 231, ; 183
	i32 300, ; 184
	i32 223, ; 185
	i32 242, ; 186
	i32 85, ; 187
	i32 214, ; 188
	i32 284, ; 189
	i32 61, ; 190
	i32 112, ; 191
	i32 57, ; 192
	i32 334, ; 193
	i32 271, ; 194
	i32 99, ; 195
	i32 180, ; 196
	i32 19, ; 197
	i32 235, ; 198
	i32 111, ; 199
	i32 101, ; 200
	i32 102, ; 201
	i32 184, ; 202
	i32 302, ; 203
	i32 104, ; 204
	i32 288, ; 205
	i32 71, ; 206
	i32 201, ; 207
	i32 38, ; 208
	i32 32, ; 209
	i32 103, ; 210
	i32 73, ; 211
	i32 308, ; 212
	i32 9, ; 213
	i32 123, ; 214
	i32 46, ; 215
	i32 225, ; 216
	i32 192, ; 217
	i32 9, ; 218
	i32 43, ; 219
	i32 4, ; 220
	i32 272, ; 221
	i32 312, ; 222
	i32 307, ; 223
	i32 31, ; 224
	i32 138, ; 225
	i32 92, ; 226
	i32 93, ; 227
	i32 327, ; 228
	i32 49, ; 229
	i32 141, ; 230
	i32 112, ; 231
	i32 140, ; 232
	i32 241, ; 233
	i32 115, ; 234
	i32 289, ; 235
	i32 204, ; 236
	i32 157, ; 237
	i32 76, ; 238
	i32 79, ; 239
	i32 261, ; 240
	i32 37, ; 241
	i32 208, ; 242
	i32 283, ; 243
	i32 175, ; 244
	i32 245, ; 245
	i32 238, ; 246
	i32 64, ; 247
	i32 138, ; 248
	i32 15, ; 249
	i32 116, ; 250
	i32 277, ; 251
	i32 286, ; 252
	i32 233, ; 253
	i32 203, ; 254
	i32 48, ; 255
	i32 70, ; 256
	i32 80, ; 257
	i32 126, ; 258
	i32 94, ; 259
	i32 121, ; 260
	i32 297, ; 261
	i32 26, ; 262
	i32 254, ; 263
	i32 97, ; 264
	i32 28, ; 265
	i32 229, ; 266
	i32 325, ; 267
	i32 303, ; 268
	i32 149, ; 269
	i32 169, ; 270
	i32 4, ; 271
	i32 98, ; 272
	i32 33, ; 273
	i32 93, ; 274
	i32 276, ; 275
	i32 189, ; 276
	i32 21, ; 277
	i32 41, ; 278
	i32 170, ; 279
	i32 319, ; 280
	i32 247, ; 281
	i32 311, ; 282
	i32 261, ; 283
	i32 296, ; 284
	i32 286, ; 285
	i32 266, ; 286
	i32 2, ; 287
	i32 134, ; 288
	i32 111, ; 289
	i32 190, ; 290
	i32 182, ; 291
	i32 331, ; 292
	i32 216, ; 293
	i32 328, ; 294
	i32 58, ; 295
	i32 95, ; 296
	i32 310, ; 297
	i32 39, ; 298
	i32 227, ; 299
	i32 25, ; 300
	i32 94, ; 301
	i32 89, ; 302
	i32 99, ; 303
	i32 292, ; 304
	i32 10, ; 305
	i32 87, ; 306
	i32 100, ; 307
	i32 273, ; 308
	i32 185, ; 309
	i32 297, ; 310
	i32 218, ; 311
	i32 307, ; 312
	i32 7, ; 313
	i32 258, ; 314
	i32 302, ; 315
	i32 215, ; 316
	i32 88, ; 317
	i32 253, ; 318
	i32 154, ; 319
	i32 306, ; 320
	i32 33, ; 321
	i32 116, ; 322
	i32 82, ; 323
	i32 20, ; 324
	i32 291, ; 325
	i32 11, ; 326
	i32 162, ; 327
	i32 3, ; 328
	i32 197, ; 329
	i32 314, ; 330
	i32 191, ; 331
	i32 190, ; 332
	i32 84, ; 333
	i32 301, ; 334
	i32 64, ; 335
	i32 316, ; 336
	i32 280, ; 337
	i32 143, ; 338
	i32 262, ; 339
	i32 157, ; 340
	i32 41, ; 341
	i32 117, ; 342
	i32 186, ; 343
	i32 217, ; 344
	i32 310, ; 345
	i32 269, ; 346
	i32 131, ; 347
	i32 75, ; 348
	i32 66, ; 349
	i32 195, ; 350
	i32 320, ; 351
	i32 172, ; 352
	i32 221, ; 353
	i32 143, ; 354
	i32 177, ; 355
	i32 106, ; 356
	i32 151, ; 357
	i32 70, ; 358
	i32 210, ; 359
	i32 156, ; 360
	i32 185, ; 361
	i32 121, ; 362
	i32 127, ; 363
	i32 315, ; 364
	i32 152, ; 365
	i32 244, ; 366
	i32 141, ; 367
	i32 231, ; 368
	i32 312, ; 369
	i32 20, ; 370
	i32 14, ; 371
	i32 135, ; 372
	i32 75, ; 373
	i32 59, ; 374
	i32 234, ; 375
	i32 167, ; 376
	i32 168, ; 377
	i32 194, ; 378
	i32 15, ; 379
	i32 74, ; 380
	i32 6, ; 381
	i32 23, ; 382
	i32 256, ; 383
	i32 209, ; 384
	i32 215, ; 385
	i32 211, ; 386
	i32 91, ; 387
	i32 313, ; 388
	i32 1, ; 389
	i32 136, ; 390
	i32 182, ; 391
	i32 257, ; 392
	i32 279, ; 393
	i32 134, ; 394
	i32 69, ; 395
	i32 146, ; 396
	i32 322, ; 397
	i32 212, ; 398
	i32 301, ; 399
	i32 211, ; 400
	i32 248, ; 401
	i32 88, ; 402
	i32 96, ; 403
	i32 238, ; 404
	i32 243, ; 405
	i32 210, ; 406
	i32 317, ; 407
	i32 31, ; 408
	i32 45, ; 409
	i32 252, ; 410
	i32 217, ; 411
	i32 109, ; 412
	i32 158, ; 413
	i32 35, ; 414
	i32 22, ; 415
	i32 114, ; 416
	i32 57, ; 417
	i32 277, ; 418
	i32 144, ; 419
	i32 118, ; 420
	i32 120, ; 421
	i32 110, ; 422
	i32 219, ; 423
	i32 139, ; 424
	i32 225, ; 425
	i32 54, ; 426
	i32 105, ; 427
	i32 323, ; 428
	i32 203, ; 429
	i32 196, ; 430
	i32 197, ; 431
	i32 133, ; 432
	i32 295, ; 433
	i32 282, ; 434
	i32 270, ; 435
	i32 329, ; 436
	i32 248, ; 437
	i32 207, ; 438
	i32 199, ; 439
	i32 159, ; 440
	i32 308, ; 441
	i32 235, ; 442
	i32 163, ; 443
	i32 132, ; 444
	i32 270, ; 445
	i32 161, ; 446
	i32 321, ; 447
	i32 259, ; 448
	i32 291, ; 449
	i32 140, ; 450
	i32 282, ; 451
	i32 278, ; 452
	i32 169, ; 453
	i32 198, ; 454
	i32 175, ; 455
	i32 220, ; 456
	i32 287, ; 457
	i32 40, ; 458
	i32 246, ; 459
	i32 81, ; 460
	i32 56, ; 461
	i32 37, ; 462
	i32 97, ; 463
	i32 166, ; 464
	i32 172, ; 465
	i32 207, ; 466
	i32 283, ; 467
	i32 82, ; 468
	i32 222, ; 469
	i32 98, ; 470
	i32 30, ; 471
	i32 159, ; 472
	i32 18, ; 473
	i32 127, ; 474
	i32 119, ; 475
	i32 242, ; 476
	i32 273, ; 477
	i32 255, ; 478
	i32 275, ; 479
	i32 293, ; 480
	i32 165, ; 481
	i32 250, ; 482
	i32 336, ; 483
	i32 272, ; 484
	i32 263, ; 485
	i32 294, ; 486
	i32 170, ; 487
	i32 16, ; 488
	i32 144, ; 489
	i32 314, ; 490
	i32 125, ; 491
	i32 118, ; 492
	i32 38, ; 493
	i32 115, ; 494
	i32 47, ; 495
	i32 142, ; 496
	i32 117, ; 497
	i32 204, ; 498
	i32 34, ; 499
	i32 178, ; 500
	i32 95, ; 501
	i32 53, ; 502
	i32 264, ; 503
	i32 129, ; 504
	i32 153, ; 505
	i32 24, ; 506
	i32 161, ; 507
	i32 241, ; 508
	i32 148, ; 509
	i32 104, ; 510
	i32 292, ; 511
	i32 89, ; 512
	i32 229, ; 513
	i32 60, ; 514
	i32 142, ; 515
	i32 100, ; 516
	i32 180, ; 517
	i32 5, ; 518
	i32 13, ; 519
	i32 122, ; 520
	i32 135, ; 521
	i32 28, ; 522
	i32 309, ; 523
	i32 72, ; 524
	i32 239, ; 525
	i32 24, ; 526
	i32 205, ; 527
	i32 227, ; 528
	i32 268, ; 529
	i32 265, ; 530
	i32 326, ; 531
	i32 137, ; 532
	i32 220, ; 533
	i32 236, ; 534
	i32 168, ; 535
	i32 269, ; 536
	i32 305, ; 537
	i32 101, ; 538
	i32 123, ; 539
	i32 240, ; 540
	i32 187, ; 541
	i32 163, ; 542
	i32 167, ; 543
	i32 243, ; 544
	i32 39, ; 545
	i32 193, ; 546
	i32 183, ; 547
	i32 313, ; 548
	i32 209, ; 549
	i32 17, ; 550
	i32 208, ; 551
	i32 171, ; 552
	i32 326, ; 553
	i32 325, ; 554
	i32 137, ; 555
	i32 150, ; 556
	i32 232, ; 557
	i32 195, ; 558
	i32 155, ; 559
	i32 130, ; 560
	i32 19, ; 561
	i32 65, ; 562
	i32 147, ; 563
	i32 47, ; 564
	i32 333, ; 565
	i32 218, ; 566
	i32 79, ; 567
	i32 61, ; 568
	i32 106, ; 569
	i32 267, ; 570
	i32 222, ; 571
	i32 49, ; 572
	i32 253, ; 573
	i32 330, ; 574
	i32 264, ; 575
	i32 14, ; 576
	i32 186, ; 577
	i32 68, ; 578
	i32 171, ; 579
	i32 228, ; 580
	i32 232, ; 581
	i32 335, ; 582
	i32 78, ; 583
	i32 237, ; 584
	i32 181, ; 585
	i32 108, ; 586
	i32 221, ; 587
	i32 263, ; 588
	i32 67, ; 589
	i32 63, ; 590
	i32 27, ; 591
	i32 160, ; 592
	i32 230, ; 593
	i32 10, ; 594
	i32 179, ; 595
	i32 173, ; 596
	i32 193, ; 597
	i32 11, ; 598
	i32 174, ; 599
	i32 78, ; 600
	i32 126, ; 601
	i32 83, ; 602
	i32 188, ; 603
	i32 66, ; 604
	i32 107, ; 605
	i32 65, ; 606
	i32 128, ; 607
	i32 122, ; 608
	i32 77, ; 609
	i32 278, ; 610
	i32 268, ; 611
	i32 334, ; 612
	i32 8, ; 613
	i32 236, ; 614
	i32 2, ; 615
	i32 44, ; 616
	i32 281, ; 617
	i32 156, ; 618
	i32 128, ; 619
	i32 266, ; 620
	i32 214, ; 621
	i32 23, ; 622
	i32 202, ; 623
	i32 213, ; 624
	i32 133, ; 625
	i32 224, ; 626
	i32 255, ; 627
	i32 294, ; 628
	i32 329, ; 629
	i32 311, ; 630
	i32 29, ; 631
	i32 179, ; 632
	i32 173, ; 633
	i32 223, ; 634
	i32 184, ; 635
	i32 62, ; 636
	i32 196, ; 637
	i32 90, ; 638
	i32 206, ; 639
	i32 87, ; 640
	i32 148, ; 641
	i32 198, ; 642
	i32 36, ; 643
	i32 86, ; 644
	i32 244, ; 645
	i32 324, ; 646
	i32 0, ; 647
	i32 319, ; 648
	i32 187, ; 649
	i32 50, ; 650
	i32 6, ; 651
	i32 202, ; 652
	i32 90, ; 653
	i32 331, ; 654
	i32 21, ; 655
	i32 162, ; 656
	i32 96, ; 657
	i32 50, ; 658
	i32 113, ; 659
	i32 260, ; 660
	i32 130, ; 661
	i32 200, ; 662
	i32 76, ; 663
	i32 27, ; 664
	i32 237, ; 665
	i32 259, ; 666
	i32 7, ; 667
	i32 194, ; 668
	i32 176, ; 669
	i32 110, ; 670
	i32 293, ; 671
	i32 260, ; 672
	i32 246 ; 673
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 4

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 4

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 4

; Functions

; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 4, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" }

; Metadata
!llvm.module.flags = !{!0, !1, !7}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"NumRegisterParameters", i32 0}
