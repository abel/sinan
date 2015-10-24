
Player=arg.Player;
index=arg.TaskList.Count-1;
for i=0,index do
	print(arg.TaskList[i]);
end



            List<Goods> list = GoodsAccess.Instance.GetGoodsList("4dba2d63a5b40f13bcac5ee9", "Task");
//            Variant d = new Variant();//=new Variant();
//            Variant b = new Variant();
//            b.Add("RoluID", 1);
//            b.Add("E", 2);
//            d.Add("Player",b);
            using (Lua lua = new Lua())
            {
                lua.NewTable("arg");

                LuaTable luat = (LuaTable)lua["arg"];

                luat["TaskList"] = list;

//                string cs = @"if arg.Player.Player.RoluID==0 then 
//                        arg.IsSuccess=0;
//                    elseif arg.Player.Player.RoluID==1 then 
//                        if arg.Player.Player.E==0 then
//                            arg.IsSuccess=1;
//                        else
//                            arg.IsSuccess=3;
//                        end
//                    else arg.IsSuccess=2;end";
                string cs = @"index=arg.TaskList.Count-1 
                for k=0,index do 
                    if arg.TaskList[k].Value.TaskID=='T_N0001' and arg.TaskList[k].Value.Status==1 then                   
                        arg.IsSuccess=true;
                        break;
    
                    end             
                end;";
                lua.DoString(cs);
                Console.WriteLine(luat["IsSuccess"]);
                //Console.WriteLine(luat["Status"]);
            }


//            List<Goods> list = GoodsAccess.Instance.GetGoodsList("4dba2d63a5b40f13bcac5ee9", "Task");
//            //            Variant d = new Variant();//=new Variant();
//            //            Variant b = new Variant();
//            //            b.Add("RoluID", 1);
//            //            b.Add("E", 2);
//            //            d.Add("Player",b);
//            using (Lua lua = new Lua())
//            {
//                lua.NewTable("arg");

//                LuaTable luat = (LuaTable)lua["arg"];

//                luat["TaskList"] = list;

//                //                string cs = @"if arg.Player.Player.RoluID==0 then 
//                //                        arg.IsSuccess=0;
//                //                    elseif arg.Player.Player.RoluID==1 then 
//                //                        if arg.Player.Player.E==0 then
//                //                            arg.IsSuccess=1;
//                //                        else
//                //                            arg.IsSuccess=3;
//                //                        end
//                //                    else arg.IsSuccess=2;end";
//                string cs = @"index=arg.TaskList.Count-1 
//                for k=0,index do 
//                    if arg.TaskList[k].Value.TaskID=='T_N0001' and arg.TaskList[k].Value.Status==1 then                   
//                        arg.IsSuccess=true;
//                        break;
//    
//                    end             
//                end;";
//                lua.DoString(cs);
//                Console.WriteLine(luat["IsSuccess"]);
//                //Console.WriteLine(luat["Status"]);
//            }