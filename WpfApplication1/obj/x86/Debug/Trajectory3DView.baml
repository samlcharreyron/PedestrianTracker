   M S B A M L   `   `   `  ����L  HPedestrianTracker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:PedestrianTrackerPedestrianTracker  5   	   (    "PedestrianTracker.Trajectory3DView   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   .myclr-namespace:PedestrianTracker     5   	     � Title$  Trajectory 3D View��5   	   $	��600��6$   $	��600��61    � Icon$ icons/pedtrac.ico��5   	   .��5      � -   5        
���   � � RowDefinitions 5   
   �� 5	      $
��Auto �6   �� 5
      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� -   5         ��Name$ button��5      $��0��6!   $��Reset��6.   6>   '� 5   
   -   5         '�Name$ viewport��5      $��1��6'    '� Camera 5      =� 5      -   5        
camera�� � LookDirection$ 0,0,-15�5       � UpDirection$ 0,1,05�6,    � Position$ 0,0,01�5      	 =� FieldOfView$	 45Q�6&   .x�5      g� -   5        	model��
 g� Content
 5      h� 5      -   5        	group��.��5      ��  �� Color$ DarkGray��6'   t� 5      $ White��6+    t� 	Direction$ -5,-5,-75�69   5      5      5      �� 5   
   -   5        $ ShowAllButton��5      $��2��6(   $��Show All��65   $��80��6f   $��Right=�6q   5       5!      