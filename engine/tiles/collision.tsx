<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.10" tiledversion="1.10.2" name="collision" tilewidth="32" tileheight="32" tilecount="60" columns="10">
 <properties>
  <property name="collision_layer_1" value="9"/>
  <property name="collision_layer_2" value="10"/>
  <property name="collision_mask_1" value=""/>
  <property name="collision_mask_2" value=""/>
 </properties>
 <image source="collision.png" width="320" height="192"/>
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
 <tile id="5">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="-3200" width="32" height="3232"/>
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
   <object id="1" x="8" y="0" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="12">
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
 <tile id="16">
  <objectgroup draworder="index" id="2">
   <object id="1" x="2" y="2" width="28" height="28">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
    <ellipse/>
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
   <object id="1" x="0" y="8" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="21">
  <objectgroup draworder="index" id="2">
   <object id="1" x="16" y="16" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="22">
  <objectgroup draworder="index" id="2">
   <object id="1" x="16" y="8" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="23">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="0" width="32" height="16">
    <properties>
     <property name="one_way" type="bool" value="true"/>
    </properties>
   </object>
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
 <tile id="30">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="16" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="31">
  <objectgroup draworder="index" id="2">
   <object id="1" x="8" y="16" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="32">
  <objectgroup draworder="index" id="2">
   <object id="1" x="16" y="16" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="36">
  <objectgroup draworder="index" id="2">
   <object id="1" x="16" y="0">
    <properties>
     <property name="physics_layer" type="int" value="2"/>
    </properties>
    <polygon points="0,1 -15,31 15,31"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="41">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="32">
    <polygon points="0,0 32,0 32,-16"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="42">
  <objectgroup draworder="index" id="3">
   <object id="3" x="32" y="0">
    <polygon points="0,0 -32,16 -32,32 0,32"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="43">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="0">
    <polygon points="0,0 32,16 32,32 0,32"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="44">
  <objectgroup draworder="index" id="2">
   <object id="1" x="32" y="32">
    <polygon points="0,0 -32,0 -32,-16"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="45">
  <objectgroup draworder="index" id="2">
   <object id="1" x="31" y="1">
    <properties>
     <property name="physics_layer" type="int" value="2"/>
    </properties>
    <polygon points="0,0 0,30 -30,15"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="46">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="8" width="32" height="24">
    <properties>
     <property name="physics_layer" type="int" value="2"/>
    </properties>
   </object>
  </objectgroup>
 </tile>
 <tile id="47">
  <objectgroup draworder="index" id="2">
   <object id="1" x="1" y="1">
    <properties>
     <property name="physics_layer" type="int" value="2"/>
    </properties>
    <polygon points="0,0 0,30 30,15"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="51">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="0">
    <polygon points="0,0 32,0 32,16"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="52">
  <objectgroup draworder="index" id="2">
   <object id="1" x="32" y="32">
    <polygon points="0,0 -32,-16 -32,-32 0,-32"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="53">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="32">
    <polygon points="0,0 32,-16 32,-32 0,-32"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="54">
  <objectgroup draworder="index" id="2">
   <object id="1" x="32" y="0">
    <polygon points="0,0 -32,0 -32,16"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="56">
  <objectgroup draworder="index" id="2">
   <object id="1" x="16" y="32">
    <properties>
     <property name="physics_layer" type="int" value="2"/>
    </properties>
    <polygon points="0,-1 -15,-31 15,-31"/>
   </object>
  </objectgroup>
 </tile>
</tileset>
