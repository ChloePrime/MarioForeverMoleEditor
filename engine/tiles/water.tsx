<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.10" tiledversion="1.11.0" name="water" tilewidth="32" tileheight="32" tilecount="24" columns="3">
 <properties>
  <property name="collision_layer_1" value="5"/>
  <property name="collision_mask_1" value=""/>
 </properties>
 <image source="water_tileset.png" width="96" height="256"/>
 <tile id="0">
  <objectgroup draworder="index" id="3">
   <object id="3" x="0" y="16" width="32" height="16">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
   </object>
  </objectgroup>
  <animation>
   <frame tileid="0" duration="100"/>
   <frame tileid="3" duration="100"/>
   <frame tileid="6" duration="100"/>
   <frame tileid="9" duration="100"/>
   <frame tileid="12" duration="100"/>
   <frame tileid="15" duration="100"/>
   <frame tileid="18" duration="100"/>
   <frame tileid="21" duration="100"/>
  </animation>
 </tile>
 <tile id="1">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="16" width="32" height="16">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
   </object>
  </objectgroup>
  <animation>
   <frame tileid="1" duration="100"/>
   <frame tileid="4" duration="100"/>
   <frame tileid="7" duration="100"/>
   <frame tileid="10" duration="100"/>
   <frame tileid="13" duration="100"/>
   <frame tileid="16" duration="100"/>
   <frame tileid="19" duration="100"/>
   <frame tileid="22" duration="100"/>
  </animation>
 </tile>
 <tile id="15">
  <objectgroup draworder="index" id="2">
   <object id="1" x="0" y="0" width="32" height="32">
    <properties>
     <property name="physics_layer" type="int" value="1"/>
    </properties>
   </object>
  </objectgroup>
 </tile>
</tileset>
