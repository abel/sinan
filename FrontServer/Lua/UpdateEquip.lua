--print("ÖÐ¹ú");
coin =100;
excount =2;
if level == 2 then
  coin=3900;
else
  coin =25;
end

if arg.OpType == 0 then
	Remove(arg.Key);
else
	arg.Equip.LV = 2;
	goodsID = arg.Equip.EquipID;
	goods = GetGoods(goodsID);
	arg.Equip.Name = goods.GoodsName;
	arg.Equip.GoodsType = goods.GoodsType;
	arg.Equips[arg.Key] = arg.Equip;
end
