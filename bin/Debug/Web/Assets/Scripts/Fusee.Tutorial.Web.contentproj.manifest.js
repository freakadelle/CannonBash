/* The size of these files were reduced. The following function fixes all references. */
var $customMSCore = JSIL.GetAssembly("mscorlib");
var $customSys = JSIL.GetAssembly("System");
var $customSysConf = JSIL.GetAssembly("System.Configuration");
var $customSysCore = JSIL.GetAssembly("System.Core");
var $customSysNum = JSIL.GetAssembly("System.Numerics");
var $customSysXml = JSIL.GetAssembly("System.Xml");
var $customSysSec = JSIL.GetAssembly("System.Security");

if (typeof (contentManifest) !== "object") { contentManifest = {}; };
contentManifest["Fusee.Tutorial.Web.contentproj"] = [
    ["Script",	"Fusee.Base.Core.Ext.js",	{  "sizeBytes": 1273 }],
    ["Script",	"Fusee.Base.Imp.Web.Ext.js",	{  "sizeBytes": 8225 }],
    ["Script",	"opentype.js",	{  "sizeBytes": 166330 }],
    ["Script",	"Fusee.Xene.Ext.js",	{  "sizeBytes": 1441 }],
    ["Script",	"Fusee.Xirkit.Ext.js",	{  "sizeBytes": 44215 }],
    ["Script",	"Fusee.Engine.Imp.Graphics.Web.Ext.js",	{  "sizeBytes": 105980 }],
    ["Script",	"SystemExternals.js",	{  "sizeBytes": 11976 }],
    ["File",	"Assets/block.fus",	{  "sizeBytes": 976 }],
    ["File",	"Assets/Bunker_v2.fus",	{  "sizeBytes": 89628 }],
    ["File",	"Assets/Bunker_v3.fus",	{  "sizeBytes": 51335 }],
    ["File",	"Assets/Bunker_v4.fus",	{  "sizeBytes": 90544 }],
    ["File",	"Assets/Bunker_v5.fus",	{  "sizeBytes": 89544 }],
    ["Image",	"Assets/Leaves.jpg",	{  "sizeBytes": 19815 }],
    ["File",	"Assets/PixelShader.frag",	{  "sizeBytes": 872 }],
    ["Image",	"Assets/Styles/loading_rocket.png",	{  "sizeBytes": 10975 }],
    ["File",	"Assets/VertexShader.vert",	{  "sizeBytes": 402 }],
    ["File",	"Assets/WuggyLand.fus",	{  "sizeBytes": 4990046 }],
    ];