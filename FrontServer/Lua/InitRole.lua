Level=2;
if Level<3 then
	arg.IsSuccess=false;
	return;
end

index=arg.TaskList.Count;

for i=1,index do
if arg.TaskList[i].Value.TaskID=='T_R0101' then
	arg.IsSuccess=false;
	return;
	end
end

	--XX--
Award={
{AwardID=0,Type='20001',SelectType='0',GoodsType='',GoodsID='',fz='',Name='Experience',Count='50'},
{AwardID=1,Type='20001',SelectType='0',GoodsType='',GoodsID='',fz='',Name='ScoreB',Count='30'}
};Finish=
{
{Type='10002',SceneID='MAP_A001',SceneName='凡海城',Total='1',Cur='0',SelectType='0',NPCName='路人甲',NPCTypeID='NPC_A0001'}
};


--split--
	
roleid=arg.Player.RoleID;
Index=table.getn(Award);
F={};
m=1;
for i=1,Index do
	local n=Award[i];
	if n.fz=='' or n.fz==roleid then
		F[m]=Award[i];
		m=m+1;
	end;
end;
Award=F;

	--split--

arg.Award=Award;
arg.Status=1;
arg.Finish=Finish;
arg.IsSuccess=true;
