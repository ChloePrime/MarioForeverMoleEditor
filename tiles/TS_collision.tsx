<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.10" tiledversion="1.10.2" name="TS_collision" tilewidth="32" tileheight="32" tilecount="50" columns="10">
 <image source="T_collision.png" width="320" height="160"/>
 <tile id="0">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="0" width="32" height="32"/>
  </objectgroup>
 </tile>
 <tile id="1">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="0" width="32" height="16"/>
  </objectgroup>
 </tile>
 <tile id="2">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="16" width="32" height="16"/>
  </objectgroup>
 </tile>
 <tile id="3">
  <objectgroup draworder="index" id="3">
   <object id="2" x="0" y="32">
    <polygon points="0,0 32,0 32,-32"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="4">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="32">
    <polygon points="0,0 32,0 0,-32"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="6">
  <objectgroup draworder="index" id="2">
   <object id="1" x="16" y="0">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
    <polygon points="0,1 -15,31 15,31"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="10">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="0" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="11">
  <objectgroup draworder="index" id="2">
   <object id="1" x="16" y="0" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="13">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="32">
    <polygon points="0,-32 32,0 32,-32"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="14">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="32">
    <polygon points="0,-32 0,0 32,-32"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="15">
  <objectgroup draworder="index" id="2">
   <object id="1" x="31" y="1">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
    <polygon points="0,0 0,30 -30,15"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="17">
  <objectgroup draworder="index" id="2">
   <object id="1" x="1" y="1">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
    <polygon points="0,0 0,30 30,15"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="20">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="16" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="21">
  <objectgroup draworder="index" id="2">
   <object id="1" x="16" y="16" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="26">
  <objectgroup draworder="index" id="2">
   <object id="1" x="16" y="32">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
    <polygon points="0,-1 -15,-31 15,-31"/>
   </object>
  </objectgroup>
 </tile>
</tileset>